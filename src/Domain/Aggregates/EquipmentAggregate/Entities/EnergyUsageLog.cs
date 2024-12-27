using Domain.Common;
using Domain.Common.Base;
using Domain.Common.Exceptions;
using Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Aggregates.EquipmentAggregate.Entities;

public class EnergyUsageLog: ValueObject
{
    public DateTime TimeStamp { get; private set; }
    public decimal Consumption { get; private set; }

    protected EnergyUsageLog() { }

    private EnergyUsageLog(decimal consumption)
    {
        ValidateConsumption(consumption);
        Consumption = consumption;
        TimeStamp = DateTime.Now;
    }

    public static EnergyUsageLog Create(decimal consumption) => new(consumption);
    private static void ValidateConsumption(decimal consumption)
    {
        if (consumption <= 0)
            throw new ArgumentException("Consumption must be greater than zero.");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TimeStamp;
        yield return Consumption;
    }
}
