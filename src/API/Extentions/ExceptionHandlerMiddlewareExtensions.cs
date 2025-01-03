using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Net;
using System.ComponentModel.DataAnnotations;
using Common.Enums;
using Common.Exceptions;
using Microsoft.IdentityModel.Tokens;
using Common;
using Domain.Common.Exceptions;
using Domain.Common.Base;

namespace API.Extentions;

public static class ExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder builder)
        => builder.UseMiddleware<CustomExceptionHandlerMiddleware>();
}
public class CustomExceptionHandlerMiddleware(RequestDelegate next, IWebHostEnvironment env, ILogger<CustomExceptionHandlerMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly IWebHostEnvironment _env = env;
    private readonly ILogger<CustomExceptionHandlerMiddleware> _logger = logger;



    public async Task Invoke(HttpContext context)
    {
        string message = null!;
        HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;
        ApiResultStatusCode apiStatusCode = ApiResultStatusCode.ServerError;

        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        string message = null!;
        HttpStatusCode httpStatusCode = HttpStatusCode.InternalServerError;
        ApiResultStatusCode apiStatusCode = ApiResultStatusCode.ServerError;

        switch (ex)
        {
            case BadRequestException badRequest:
                _logger.LogError(badRequest, badRequest.Message);
                httpStatusCode = badRequest.HttpStatusCode;
                apiStatusCode = badRequest.ApiStatusCode;
                message = CreateDetailedErrorMessage(badRequest);
                break;

            case AppException appException:
                _logger.LogError(appException, appException.Message);
                httpStatusCode = appException.HttpStatusCode;
                apiStatusCode = appException.ApiStatusCode;
                message = CreateDetailedErrorMessage(appException);
                break;

            case SecurityTokenExpiredException tokenExpiredException:
                _logger.LogError(tokenExpiredException, tokenExpiredException.Message);
                SetUnauthorizedResponse(tokenExpiredException, out httpStatusCode, out apiStatusCode, ref message);
                break;

            case UnauthorizedAccessException unauthorizedAccessException:
                _logger.LogError(unauthorizedAccessException, unauthorizedAccessException.Message);
                SetUnauthorizedResponse(unauthorizedAccessException, out httpStatusCode, out apiStatusCode, ref message);
                break;

            case ValidationException validationException:
                _logger.LogError(validationException, validationException.Message);
                httpStatusCode = HttpStatusCode.BadRequest;
                apiStatusCode = ApiResultStatusCode.BadRequest;
                message = validationException.Message;
                break;

            case DomainException<IEntity> domainException:
                _logger.LogError(domainException, $"Domain Error in {domainException.DomainName}: {domainException.Message}");
                httpStatusCode = HttpStatusCode.BadRequest;
                apiStatusCode = ApiResultStatusCode.DomainError;
                message = CreateDetailedErrorMessage(domainException);
                break;

            default:
                _logger.LogError(ex, ex.Message);
                if (_env.IsDevelopment())
                {
                    var details = new Dictionary<string, string>
                    {
                        ["Exception"] = ex.Message,
                        ["StackTrace"] = ex.StackTrace ?? string.Empty
                    };
                    if (ex.InnerException != null)
                    {
                        details.Add("InnerException.Exception", ex.InnerException.Message);
                        details.Add("InnerException.StackTrace", ex.InnerException.StackTrace ?? string.Empty);
                    }
                    message = JsonConvert.SerializeObject(details);
                }
                else if (ex.InnerException?.Message.Contains("ORA-00001: unique constraint") is true)
                {
                    apiStatusCode = ApiResultStatusCode.InformationExists;
                }
                break;
        }

        await WriteToResponseAsync(context, httpStatusCode, apiStatusCode, message);
    }

    private string CreateDetailedErrorMessage(Exception ex)
    {
        if (_env.IsDevelopment())
        {
            Dictionary<string, string> details = new() 
            {
                ["Exception"] = ex.Message,
                ["StackTrace"] = ex.StackTrace ?? string.Empty
            };
            if (ex.InnerException is not null)
            {
                details.Add("InnerException.Exception", ex.InnerException.Message);
                details.Add("InnerException.StackTrace", ex.InnerException.StackTrace ?? string.Empty);
            }

            if (ex is BadRequestException badRequest && badRequest.AdditionalData is not null)
                details.Add("AdditionalData", JsonConvert.SerializeObject(badRequest.AdditionalData));


            return JsonConvert.SerializeObject(details);
        }

        return ex.Message;
    }

    private void SetUnauthorizedResponse(Exception ex, out HttpStatusCode statusCode, out ApiResultStatusCode apiStatusCode, ref string message)
    {
        statusCode = HttpStatusCode.Unauthorized;
        apiStatusCode = ApiResultStatusCode.UnAuthorized;

        if (_env.IsDevelopment())
        {
            var details = new Dictionary<string, string>
            {
                ["Exception"] = ex.Message,
                ["StackTrace"] = ex.StackTrace ?? string.Empty
            };

            if (ex is SecurityTokenExpiredException tokenExpiredException)
                details.Add("Expires", tokenExpiredException.Expires.ToString());


            message = JsonConvert.SerializeObject(details);
        }
    }

    private async Task WriteToResponseAsync(HttpContext context, HttpStatusCode httpStatusCode, ApiResultStatusCode apiStatusCode, string message)
    {
        if (context.Response.HasStarted)
            throw new InvalidOperationException("The response has already started, the HTTP status code middleware will not be executed.");

        ApiResult result = new(false, apiStatusCode, message);
        var json = JsonConvert.SerializeObject(result);

        context.Response.StatusCode = (int)httpStatusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(json);
    }
}
