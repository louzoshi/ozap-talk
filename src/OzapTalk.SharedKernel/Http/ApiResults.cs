using Microsoft.AspNetCore.Http;
using OzapTalk.SharedKernel.Results;

namespace OzapTalk.SharedKernel.Http;

/// <summary>Maps a domain <see cref="Result"/> onto an HTTP response with RFC 7807 problem details.</summary>
public static class ApiResults
{
    public static IResult Problem(Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return TypedResults.Problem(
            detail: error.Description,
            statusCode: status,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }

    public static IResult Match<TValue>(this Result<TValue> result, Func<TValue, IResult> onSuccess) =>
        result.IsSuccess ? onSuccess(result.Value) : Problem(result.Error);

    public static IResult Match(this Result result, Func<IResult> onSuccess) =>
        result.IsSuccess ? onSuccess() : Problem(result.Error);
}
