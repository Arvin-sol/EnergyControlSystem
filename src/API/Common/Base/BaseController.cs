using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Common.Base;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class BaseController(IMediator mediatR) : Controller
{
    public bool UserIsAuthenticated => HttpContext.User.Identity!.IsAuthenticated;
    protected readonly IMediator _mediatR = mediatR;
    public ulong CreatorId => User.Identities

}