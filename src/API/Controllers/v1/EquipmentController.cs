using API.Common.Base;
using Application.Commands;
using Application.Dtos.EquipmentDTOs;
using Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers.v1;


public class EquipmentController : BaseController
{
    public EquipmentController(IMediator mediatR) : base(mediatR)
    {
    }

    public async Task<ApiResult> CreateEquipment(EquipmentCuDTO dto, CancellationToken cancellationToken)
        => await _mediatR.Send(new CreateEquipmentCommand(dto), cancellationToken);

}
