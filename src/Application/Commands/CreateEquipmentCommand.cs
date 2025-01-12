using Application.Dtos.EquipmentDTOs;
using Common;
using Common.Enums;
using Domain.Aggregates.EquipmentAggregate.Contracts;
using Domain.Aggregates.EquipmentAggregate.Entities;
using MediatR;
using System.Reflection.Metadata.Ecma335;

namespace Application.Commands;



public record CreateEquipmentCommand(EquipmentCuDTO dto) : IRequest<ApiResult>
{
    public class EquipmentHandler(IEquipmentRepository equipmentRepository) : IRequestHandler<CreateEquipmentCommand,ApiResult>
    {
        private readonly IEquipmentRepository _equipmentRepository = equipmentRepository;
        public async Task<ApiResult> Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
        {
            if (request.dto is null)
                throw new ArgumentNullException(nameof(request.dto), "The equipment DTO must not be null.");

            Equipment newEquipment = Equipment.Create(
                name: request.dto.Name,
                type: request.dto.Type);

            await _equipmentRepository.CreateEquipmentAsync(newEquipment, cancellationToken);
            return new ApiResult(true,ApiResultStatusCode.Success);

        }
    }
}