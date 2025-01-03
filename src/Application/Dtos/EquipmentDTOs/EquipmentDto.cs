using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Domain.Aggregates.EquipmentAggregate.Enums.EquipmentEnums;

namespace Application.Dtos.EquipmentDTOs;

public class EquipmentCuDTO
{
    public required string Name { get; set; }
    public required EquipmentType Type { get; set; }
}
