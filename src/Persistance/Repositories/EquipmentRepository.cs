using Domain.Aggregates.EquipmentAggregate.Contracts;
using Domain.Aggregates.EquipmentAggregate.Entities;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;

namespace Persistance.Repositories;

public class EquipmentRepository : IEquipmentRepository
{
    private readonly IInfluxDBClient _influxDbClient;
    private readonly ILogger<EquipmentRepository> _logger;
    private const string EquipmentMeasurement = "equipment";
    private const string UsageMeasurement = "energy_usage_log";
    private const string Bucket = "your-bucket";

    public EquipmentRepository(IInfluxDBClient influxDbClient, ILogger<EquipmentRepository> logger)
    {
        _influxDbClient = influxDbClient;
        _logger = logger;
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
}

