

using Common.Enums;
using Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Common;

public class ApiResult
{
    public bool IsSuccess { get; set; }
    public ApiResultStatusCode StatusCode { get; set; }

    public string Message { get; set; }

    public int? TotalItemCount { get; set; }

    public int? PageNumber { get; set; }

    public int? PageSize { get; set; }

    public int? PageCount { get; set; }

    public ApiResult(bool isSuccess, ApiResultStatusCode statusCode, string message = null)
    {
        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message ?? statusCode.ToDisplay();
    }


    #region Implicit Operators

    public static implicit operator ApiResult(BadRequestObjectResult result)
    {
        var message = result.Value?.ToString();
        if (result.Value is SerializableError errors)
        {
            var errorMessages = errors.SelectMany(p => (string[])p.Value).Distinct();
            message = string.Join(" | ", errorMessages);
        }
        return new ApiResult(false, ApiResultStatusCode.BadRequest, message!);
    }

    public static implicit operator ApiResult(OkResult result) => new ApiResult(true, ApiResultStatusCode.Success);


    public static implicit operator ApiResult(BadRequestResult result) => new ApiResult(false, ApiResultStatusCode.BadRequest);

    public static implicit operator ApiResult(ContentResult result) => new ApiResult(true, ApiResultStatusCode.Success, result.Content);

    public static implicit operator ApiResult(NotFoundResult result) => new ApiResult(false, ApiResultStatusCode.NotFound);

    #endregion
}

public class ApiResult<TData> : ApiResult
    where TData : class
{
    [JsonProperty("data")]
    public TData Data { get; set; }

    public ApiResult(bool isSuccess, ApiResultStatusCode statusCode, TData data, string message = null!, int? totalItemCount = null, int? pageNumber = null, int? pageSize = null, int? pageCount = null)
        : base(isSuccess, statusCode, message)
    {
        Data = data;
        PageNumber = pageNumber;
        TotalItemCount = totalItemCount;
        PageSize = pageSize;
        PageCount = pageCount;
    }

    #region Implicit Operators

    public static implicit operator ApiResult<TData>(BadRequestObjectResult result)
    {
        var message = result.Value?.ToString();
        if (result.Value is SerializableError errors)
        {
            var errorMessages = errors.SelectMany(p => (string[])p.Value).Distinct();
            message = string.Join(" | ", errorMessages);
        }
        return new ApiResult<TData>(false, ApiResultStatusCode.BadRequest, null!, message);
    }

    public static implicit operator ApiResult<TData>(TData data) => new ApiResult<TData>(true, ApiResultStatusCode.Success, data);


    public static implicit operator ApiResult<TData>(OkResult result) => new ApiResult<TData>(true, ApiResultStatusCode.Success, null!);


    public static implicit operator ApiResult<TData>(OkObjectResult result) => new ApiResult<TData>(true, ApiResultStatusCode.Success, (TData)result.Value);


    public static implicit operator ApiResult<TData>(BadRequestResult result) => new ApiResult<TData>(false, ApiResultStatusCode.BadRequest, null!);


    public static implicit operator ApiResult<TData>(ContentResult result) => new ApiResult<TData>(true, ApiResultStatusCode.Success, null!, result.Content);


    public static implicit operator ApiResult<TData>(NotFoundResult result) => new ApiResult<TData>(false, ApiResultStatusCode.NotFound, null!);


    public static implicit operator ApiResult<TData>(NotFoundObjectResult result) => new ApiResult<TData>(false, ApiResultStatusCode.NotFound, (TData)result.Value);

    #endregion
}