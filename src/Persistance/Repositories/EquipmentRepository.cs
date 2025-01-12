using Common.Extension;
using Domain.Aggregates.EquipmentAggregate.Contracts;
using Domain.Aggregates.EquipmentAggregate.Entities;
using Domain.Common.Exceptions;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using static Domain.Aggregates.EquipmentAggregate.Enums.EquipmentEnums;

namespace Persistance.Repositories;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly IInfluxDBClient _influxDbClient;
    private readonly ILogger<EquipmentRepository> _logger;
    private readonly IConnectionMultiplexer _redisDb;
    private const string EquipmentMeasurement = "Equipment";
    private const string UsageMeasurement = "EnergyUsageLog";
    private const string Bucket = "EnergyDataBucket";
    private const string Org = "EnergyOrg"; 
    private const string Token = "your-token";
    private const string Url = "http://localhost:8086"; 
    public EquipmentRepository(IInfluxDBClient influxDbClient, ILogger<EquipmentRepository> logger, IConnectionMultiplexer redisDb)
    {
        _influxDbClient = influxDbClient;
        _logger = logger;
        _redisDb = redisDb;
    }

    public async Task CreateEquipmentAsync(Equipment entity, CancellationToken cancellationToken)
    {
        try
        {

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var writeApi = _influxDbClient.GetWriteApiAsync();

            var equipmentPoint = PointData.Measurement(EquipmentMeasurement)
                                          .Tag("name", entity.Name)
                                          .Tag("type", entity.Type.ToString())
                                          .Field("last_updated", entity.LastUpdated.ToString("o")) 
                                          .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            await writeApi.WritePointAsync(equipmentPoint, Bucket);

            _logger.LogInformation($"Equipment {entity.Name} created successfully in InfluxDB.");
        }
        catch (InfluxException influxEx)
        {
            _logger.LogError($"InfluxDB error while creating equipment {entity.Name}: {influxEx.Message}");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating equipment {entity.Name}: {ex.Message}");
            throw;
        }
    }
    public async Task LogEquipmentUsageAsync(ulong equipmentId, decimal consumption, CancellationToken cancellationToken)
    {
        try
        {
            if (consumption <= 0)
                throw new ArgumentException("Energy consumption must be greater than zero.", nameof(consumption));

            var logPoint = PointData.Measurement(UsageMeasurement)
                                    .Tag("equipment_id", equipmentId.ToString())
                                    .Field("consumption", consumption)
                                    .Timestamp(DateTime.Now, WritePrecision.Ns);

            var writeApi = _influxDbClient.GetWriteApiAsync();
            await writeApi.WritePointAsync(logPoint, Bucket,cancellationToken:cancellationToken);

            _logger.LogInformation("Logged energy usage for Equipment ID: {EquipmentId}, Consumption: {Consumption}",
                                    equipmentId, consumption);
        }
        catch (InfluxException influxEx)
        {
            _logger.LogError("InfluxDB error while logging usage for Equipment ID {EquipmentId}: {ErrorMessage}",
                             equipmentId, influxEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error logging energy usage for Equipment ID {EquipmentId}: {ErrorMessage}",
                             equipmentId, ex.Message);
            throw;
        }
    }



    public async Task LogEnergyUsageForEquipmentAsync(Equipment equipment, decimal consumption, CancellationToken cancellationToken)
    {
        try
        {
            if (equipment == null)
                throw new ArgumentNullException(nameof(equipment));

            if (consumption <= 0)
                throw new ArgumentException("Energy consumption must be greater than zero.", nameof(consumption));

            equipment.LogEnergyUsage(consumption);

            var logPoint = PointData.Measurement(UsageMeasurement)
                                    .Tag("equipment_id", equipment.Id.ToString())
                                    .Tag("equipment_name", equipment.Name)
                                    .Tag("equipment_type", equipment.Type.ToString())
                                    .Field("consumption", consumption)
                                    .Field("timestamp", equipment.LastUpdated.ToString("o"))
                                    .Timestamp(DateTime.Now, WritePrecision.Ns);

            var writeApi = _influxDbClient.GetWriteApiAsync();
            await writeApi.WritePointAsync(logPoint, Bucket);

        }
        catch (InfluxException influxEx)
        {
            _logger.LogError("InfluxDB error while logging usage for Equipment ID {EquipmentId}: {ErrorMessage}",
                             equipment?.Id, influxEx.Message);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error logging energy usage for Equipment ID {EquipmentId}: {ErrorMessage}",
                             equipment?.Id, ex.Message);
            throw;
        }
    }
    public async Task<IReadOnlyCollection<Equipment>> GetEquipmentsUsageAsync(CancellationToken cancellationToken)
    {
        var cachedData = await _redisDb.GetDatabase().StringGetAsync("equipments_usage");

        if (cachedData.HasValue)
            return DeserializeFromCache(cachedData!);

        var equipmentsData = await QueryEquipmentsDataAsync(cancellationToken);

        if (equipmentsData != null && equipmentsData.Any())
        {
            var serializedData = SerializeToCache(equipmentsData);
            await _redisDb.GetDatabase().StringSetAsync("equipments_usage", serializedData, TimeSpan.FromMinutes(10));
        }

        return equipmentsData!;
    }

    #region Private Methods
    private async Task<IReadOnlyCollection<Equipment>> QueryEquipmentsDataAsync(CancellationToken cancellationToken)
    {
        string fluxQuery = $@"
            from(bucket: ""{Bucket}"")
            |> range(start: -30d) // Last 30 days
            |> filter(fn: (r) => r._measurement == ""{UsageMeasurement}"")
            |> keep(columns: [""equipment_id"", ""consumption"", ""timestamp""])";

        var queryApi = _influxDbClient.GetQueryApi();
        var fluxTables = await queryApi.QueryAsync(fluxQuery, Org, cancellationToken: cancellationToken);

        List<Equipment> equipments = new();

        foreach (var table in fluxTables)
        {
            foreach (var record in table.Records)
            {
                var equipment = CreateEquipmentFromRecord(record);
                if (equipment != null)
                    equipments.Add(equipment);
            }
        }

        return equipments.AsReadOnly();
    }

    private Equipment? CreateEquipmentFromRecord(FluxRecord record)
    {
        var equipmentName = record.GetValueByKey("equipment_name")?.ToString();
        var equipmentTypeString = record.GetValueByKey("equipment_type")?.ToString();
        var lastUpdatedString = record.GetValueByKey("last_updated")?.ToString();
        var consumptionString = record.GetValueByKey("consumption")?.ToString();

        if (equipmentTypeString!.IsValidEnum(out EquipmentType equipmentType) &&
            lastUpdatedString!.IsValidDateTime(out DateTime lastUpdated) &&
            !string.IsNullOrEmpty(equipmentName))
        {
            var equipment = Equipment.Create(equipmentName, equipmentType);
            typeof(Equipment).GetProperty("LastUpdated", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                              ?.SetValue(equipment, lastUpdated);

            if (consumptionString!.IsValidDecimal(out var consumption) && consumption > 0)
                equipment.LogEnergyUsage(consumption);

            return equipment;
        }
        return null;
    }

    private string SerializeToCache(IEnumerable<Equipment> data)
        => System.Text.Json.JsonSerializer.Serialize(data);

    private IReadOnlyCollection<Equipment> DeserializeFromCache(string data)
        => System.Text.Json.JsonSerializer.Deserialize<List<Equipment>>(data)!;
    #endregion


}


