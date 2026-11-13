using System.Diagnostics;
using Altairis.Backoffice.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Altairis.Backoffice.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<T> OkOrProblem<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : ProblemFor(result.Error);

    protected ObjectResult ProblemFor(Error error)
    {
        int statusCode = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => throw new UnreachableException($"Error type '{error.Type}' has no HTTP status mapping."),
        };

        return Problem(
            detail: error.Message,
            statusCode: statusCode,
            extensions: new Dictionary<string, object?> { ["code"] = error.Code });
    }
}
