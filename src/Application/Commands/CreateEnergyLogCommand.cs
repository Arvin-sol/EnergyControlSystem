using Application.Dtos.EquipmentDTOs;
using Domain.Aggregates.EquipmentAggregate.Contracts;
using Domain.Aggregates.EquipmentAggregate.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands;

public record CreateEnergyLogCommand(EnergyLogCuDTO dto,ulong equipmentId):IRequest
{
    public class CreateEnergyLogHandler(IEquipmentRepository equipmentRepository) : IRequestHandler<CreateEnergyLogCommand>
    {
        private readonly IEquipmentRepository _equipmentRepository = equipmentRepository;
        public async Task Handle(CreateEnergyLogCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.dto.Consumption <= 0)
                    throw new ArgumentException("Consumption must be greater than zero.");

                await _equipmentRepository.LogEquipmentUsageAsync(request.equipmentId,request.dto.Consumption ,cancellationToken);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while creating the energy log.", ex);
            }
        }
    }
}
