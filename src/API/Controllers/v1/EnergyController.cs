using API.Common.Base;
using Application.Commands;
using Application.Dtos.EquipmentDTOs;
using Application.Dtos.Messages;
using Application.Queries;
using Common;
using Common.Enums;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;

public class EnergyController : BaseController
{
    private readonly IBus _bus;
    public EnergyController(IMediator mediatR, IBus bus) : base(mediatR)
    {
        _bus = bus;
    }

    public async Task<ApiResult> CreateEnergyLog(EnergyLogCuDTO dto, CancellationToken cancellationToken)
    {
        await _bus.Publish(new CreateEnergyLogMessage { EnergyLog = dto ,EquipmentId = CreatorId});
        return new ApiResult(true, ApiResultStatusCode.Success);
    }

    public async Task<ApiResult> Status(CancellationToken cancellationToken)
        => await _mediatR.Send(new GetEnergyUsageLogQuery(), cancellationToken);

}
