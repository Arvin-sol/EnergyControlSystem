using Domain.Aggregates.EquipmentAggregate.Contracts;
using Domain.Aggregates.EquipmentAggregate.Entities;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

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

    private async Task<IReadOnlyCollection<Equipment>> QueryEquipmentsDataAsync(CancellationToken cancellationToken)
    {
        string fluxQuery = @"
            from(bucket: ""your-bucket"")
                |> range(start: -30d) // Last 30 days
                |> filter(fn: (r) => r._measurement == ""energy_usage"")
                |> keep(columns: [""equipment_name"", ""is_active"", ""consumption""])";


        var queryApi = _influxDbClient.GetQueryApi();
        var result = await queryApi.QueryAsync(fluxQuery, "your-org", cancellationToken: cancellationToken);


        var equipmentList = result.ToList();

        return equipmentList;
    }

    private string SerializeToCache(IEnumerable<Equipment> data) 
        => System.Text.Json.JsonSerializer.Serialize(data);

    private IReadOnlyCollection<Equipment> DeserializeFromCache(string data) 
        => System.Text.Json.JsonSerializer.Deserialize<List<Equipment>>(data)!;


}


