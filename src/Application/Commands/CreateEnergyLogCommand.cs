using Application.Dtos.EquipmentDTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands;

public record CreateEnergyLogCommand(EnergyLogCuDTO dto):IRequest
{
    public class CreateEnergyLogHandler : IRequestHandler<CreateEnergyLogCommand>
    {
        public Task Handle(CreateEnergyLogCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
