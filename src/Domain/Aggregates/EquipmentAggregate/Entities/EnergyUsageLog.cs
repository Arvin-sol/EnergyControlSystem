using Domain.Common;
using Domain.Common.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.EquipmentAggregate.Entities;

public class EnergyUsageLog:BaseEntity<ulong>
{
    public decimal Consumption { get; private set; }
    public DateTime Timestamp { get; private set; }

    protected EnergyUsageLog() { }

    private EnergyUsageLog(decimal consumption)
    {
        ValidateConsumption(consumption);
        Consumption = consumption;
        Timestamp = DateTime.Now;
    }

    public static EnergyUsageLog Create(decimal consumption) => new(consumption);
    private static void ValidateConsumption(decimal consumption)
    {
        if (consumption <= 0)
            throw new DomainException<EnergyUsageLog>("Consumption must be greater than zero.");
    }
}
