

using static Domain.Aggregates.EquipmentAggregate.Enums.EquipmentEnums;

namespace Application.Dtos.EquipmentDTOs;

public record EquipmentListDTO(string Name, bool IsActive, EquipmentType Type, DateTime LastUpdated, decimal ConsumptionSum);