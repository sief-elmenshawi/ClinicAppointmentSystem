using Clinic.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Clinic.API;

public static class ResultExtensions
{
    public static IActionResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return result.ErrorType switch
        {
            ErrorType.NotFound => new NotFoundObjectResult(result.Error),
            ErrorType.Forbidden => new ObjectResult(result.Error) { StatusCode = StatusCodes.Status403Forbidden },
            ErrorType.Conflict => new ConflictObjectResult(result.Error),
            _ => new BadRequestObjectResult(result.Error)
        };
    }

    public static IActionResult ToHttpActionResult<T>(this Result<T> result)
    {
        return result.IsSuccess ? new OkResult() : result.ToHttpResult();
    }
}