using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.EquipmentDTOs;

namespace Application.Dtos.Messages;

public record CreateEnergyLogMessage
{
    public required ulong EquipmentId { get; init; }
    public required EnergyLogCuDTO EnergyLog { get; init; }
}