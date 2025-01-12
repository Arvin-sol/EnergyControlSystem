using Application.Dtos.EquipmentDTOs;
using Domain.Aggregates.EquipmentAggregate.Contracts;
using Domain.Aggregates.EquipmentAggregate.Entities;
using MediatR;

namespace Application.Commands;



public record CreateEquipmentCommand(EquipmentCuDTO dto) : IRequest
{
    public class EquipmentHandler(IEquipmentRepository equipmentRepository) : IRequestHandler<CreateEquipmentCommand>
    {
        private readonly IEquipmentRepository _equipmentRepository = equipmentRepository;
        public async Task Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
        {
            if (request.dto is null)
                throw new ArgumentNullException(nameof(request.dto), "The equipment DTO must not be null.");

            Equipment newEquipment = Equipment.Create(
                name: request.dto.Name,
                type: request.dto.Type);

            await _equipmentRepository.CreateEquipmentAsync(newEquipment, cancellationToken);

        }
    }
}