

using Application.Dtos.EquipmentDTOs;
using Common;
using Common.Enums;
using Domain.Aggregates.EquipmentAggregate.Contracts;
using MediatR;

namespace Application.Queries;

public record GetEnergyUsageLogQuery : IRequest<ApiResult<List<EquipmentListDTO>>>
{
    public class GetEnergyUsageLogHandler(IEquipmentRepository equipmentRepository) : IRequestHandler<GetEnergyUsageLogQuery, ApiResult<List<EquipmentListDTO>>>
    {
        private readonly IEquipmentRepository _equipmentRepository = equipmentRepository;
        public async Task<ApiResult<List<EquipmentListDTO>>> Handle(GetEnergyUsageLogQuery request, CancellationToken cancellationToken)
        {

            var result = await _equipmentRepository.GetEquipmentsUsageAsync(cancellationToken);
            if(result is null || result.Count() is 0)
                return new ApiResult<List<EquipmentListDTO>>(false, ApiResultStatusCode.NotFound, null!);

            List<EquipmentListDTO> equipmentListDTOs = new();
            foreach (var item in result)
            {
                EquipmentListDTO dto = new(
                    Name: item.Name,
                    IsActive: item.IsActive,
                    Type:item.Type,
                    LastUpdated: item.LastUpdated,
                    ConsumptionSum: item.Consumption);
                equipmentListDTOs.Add(dto);
            }
            return new ApiResult<List<EquipmentListDTO>>(true,ApiResultStatusCode.Success, equipmentListDTOs);

        }
    }
}
