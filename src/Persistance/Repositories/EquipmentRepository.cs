using Common.Extension;
using Domain.Aggregates.EquipmentAggregate.Contracts;
using Domain.Aggregates.EquipmentAggregate.Entities;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
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
    private const string EquipmentMeasurement = "equipment";
    private const string UsageMeasurement = "energy_usage_log";
    private const string Bucket = "your-bucket";

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
            var writeApi = _influxDbClient.GetWriteApiAsync();

            var equipmentPoint = PointData.Measurement(EquipmentMeasurement)
                                          .Tag("name", entity.Name)
                                          .Tag("type", entity.Type.ToString())
                                          .Field("last_updated", entity.LastUpdated)
                                          .Timestamp(DateTime.UtcNow, WritePrecision.Ns); 

            await writeApi.WritePointAsync(equipmentPoint, Bucket);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error creating equipment {entity.Name}: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateEquipmentAsync(Equipment entity, CancellationToken cancellationToken)
    {
        try
        {
            var writeApi = _influxDbClient.GetWriteApiAsync();
            var equipmentPoint = PointData.Measurement(EquipmentMeasurement)
                                          .Tag("name", entity.Name)
                                          .Tag("type", entity.Type.ToString())
                                          .Field("last_updated", entity.LastUpdated)
                                          .Timestamp(DateTime.UtcNow, WritePrecision.Ns);  

            await writeApi.WritePointAsync(equipmentPoint, Bucket);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating equipment {entity.Name}: {ex.Message}");
            throw;
        }
    }

    public async Task<IReadOnlyCollection<Equipment>> GetEquipmentsUsageAsync(CancellationToken cancellationToken)
    {
        var cachedData = await _redisDb.GetDatabase().StringGetAsync("equipments_usage");

        if (cachedData.HasValue)
            DeserializeFromCache(cachedData!);


        var equipmentsData = await QueryEquipmentsDataAsync(cancellationToken);

        if (equipmentsData is not null && equipmentsData.Any())
        {
            var serializedData = SerializeToCache(equipmentsData);
            await _redisDb.GetDatabase().StringSetAsync("equipments_usage", serializedData, TimeSpan.FromMinutes(10));
        }

        return equipmentsData!;
    }
    #region Private Methods
    private async Task<IReadOnlyCollection<Equipment>> QueryEquipmentsDataAsync(CancellationToken cancellationToken)
    {
        string fluxQuery = @"
        from(bucket: ""your-bucket"")
        |> range(start: -30d) // Last 30 days
        |> filter(fn: (r) => r._measurement == ""energy_usage"")
        |> keep(columns: [""equipment_name"", ""equipment_type"", ""last_updated"", ""consumption""])";

        var queryApi = _influxDbClient.GetQueryApi();
        var fluxTables = await queryApi.QueryAsync(fluxQuery, "your-org", cancellationToken: cancellationToken);

        List<Equipment> equipments = new();

        foreach (var table in fluxTables)
        {
            foreach (var record in table.Records)
            {
                var equipment = CreateEquipmentFromRecord(record);
                if (equipment is not null)
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


