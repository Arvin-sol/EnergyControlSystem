using Application.Dtos.EquipmentDTOs;
using MediatR;

namespace Application.Commands;



public record CreateEquipmentCommand(EquipmentCuDTO dto) : IRequest
{
    public class EquipmentHandler : IRequestHandler<CreateEquipmentCommand>
    {
        public Task Handle(CreateEquipmentCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}