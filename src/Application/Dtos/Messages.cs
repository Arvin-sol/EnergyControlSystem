using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos;

public record CreateEnergyLogMessage
{
    public required EnergyLogCuDTO EnergyLog { get; init; }
}