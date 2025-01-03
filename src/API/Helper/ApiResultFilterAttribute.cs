using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Common;
using Common.Enums;

namespace API.Helper;


public class ApiResultFilterAttribute : ActionFilterAttribute
{
    public override void OnResultExecuting(ResultExecutingContext context)
    {
        context.Result = context.Result switch
        {
            OkObjectResult okObjectResult => CreateApiResult(okObjectResult.Value, ApiResultStatusCode.Success, okObjectResult.StatusCode),
            OkResult okResult => CreateApiResult(null, ApiResultStatusCode.Success, okResult.StatusCode),
            ObjectResult { StatusCode: 400 } badRequestResult => HandleBadRequestResult(badRequestResult),
            ObjectResult { StatusCode: 404 } notFoundResult => HandleNotFoundResult(notFoundResult),
            ContentResult contentResult => CreateApiResult(contentResult.Content, ApiResultStatusCode.Success, contentResult.StatusCode),
            ObjectResult { StatusCode: null, Value: not ApiResult } objectResult => CreateApiResult(objectResult.Value, ApiResultStatusCode.Success, objectResult.StatusCode),
            _ => context.Result
        };

        base.OnResultExecuting(context);
    }

    private JsonResult CreateApiResult(object? value, ApiResultStatusCode statusCode, int? statusCodeOverride = null)
    {
        ApiResult<object> apiResult = new(true, statusCode, value!);
        return new JsonResult(apiResult) { StatusCode = statusCodeOverride };
    }

    private JsonResult HandleBadRequestResult(ObjectResult badRequestResult)
    {
        string message = badRequestResult.Value switch
        {
            ValidationProblemDetails validationDetails => string.Join(" | ", validationDetails.Errors.SelectMany(p => p.Value).Distinct()),
            SerializableError errors => string.Join(" | ", errors.SelectMany(p => (string[])p.Value).Distinct()),
            { } value when value is not ProblemDetails => value.ToString()!,
            _ => string.Empty
        };

        ApiResult apiResult = new(false, ApiResultStatusCode.BadRequest, message);
        return new JsonResult(apiResult) { StatusCode = badRequestResult.StatusCode };
    }

    private JsonResult HandleNotFoundResult(ObjectResult notFoundResult)
    {
        string? message = notFoundResult.Value is not ProblemDetails ? notFoundResult.Value?.ToString() : null;
        ApiResult apiResult = new(false, ApiResultStatusCode.NotFound, message!);
        return new JsonResult(apiResult) { StatusCode = notFoundResult.StatusCode };
    }
}
