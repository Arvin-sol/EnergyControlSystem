using Domain.Common;
using Domain.Common.Base;
using static Domain.Aggregates.EquipmentAggregate.Enums.EquipmentEnums;

namespace Domain.Aggregates.EquipmentAggregate.Entities;

public class Equipment : BaseEntity<ulong>,IAggregateRoot
{
    public string Name { get; private set; }
    public EquipmentType Type { get; private set; }
    public DateTime LastUpdated { get; private set; }

    private readonly List<EnergyUsageLog> _usageLogs = new();
    public IReadOnlyCollection<EnergyUsageLog> UsageLogs => _usageLogs.AsReadOnly();

    protected Equipment() { }

    private Equipment(string name, EquipmentType type)
    {
        Name = name;
        Type = type;
    }

    public static Equipment Create(string name, EquipmentType type) => new(name, type);

    public void LogEnergyUsage(decimal consumption)
    {
        if (consumption <= 0)
            throw new DomainException<Equipment>("Energy consumption must be greater than zero.");

        var log = EnergyUsageLog.Create(consumption);
        _usageLogs.Add(log);
        LastUpdated = DateTime.Now;
    }
    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException<Equipment>("Equipment name is required.");
    }

    private static void ValidateConsumption(decimal consumption)
    {
        if (consumption <= 0)
            throw new DomainException<Equipment>("Energy consumption must be greater than zero.");
    }
}
