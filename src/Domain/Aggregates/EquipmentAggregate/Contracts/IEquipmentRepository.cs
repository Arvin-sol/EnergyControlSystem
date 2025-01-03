

using Domain.Aggregates.EquipmentAggregate.Entities;
using Domain.Common.Contracts;

namespace Domain.Aggregates.EquipmentAggregate.Contracts;

public interface IEquipmentRepository:IScopedDependency
{
    Task CreateEquipmentAsync(Equipment entity, CancellationToken cancellationToken);
    Task UpdateEquipmentAsync(Equipment entity, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Equipment>> GetEquipmentsUsageAsync(CancellationToken cancellationToken);
}
