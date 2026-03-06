using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Park.BusinessLogic.Exceptions;

namespace obligatorio.WebApi.Filters;

public sealed class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext ctx )
    {
        var ex = ctx.Exception;

        switch (ex)
        {
            // 409
            case DuplicateEmailException:
            case AgeRequirementNotMetException:
            case AttractionCapacityReachedException:
            case AttractionDisabledException:
            case SpecialEventAttractionMismatchException:
                Write(ctx, new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://errors.yourapi.com/user.email.duplicate"
                }, code: "user.email.duplicate");
                return;

            case InvalidOperationException:
                Write(ctx, new ProblemDetails
                {
                    Title = "Conflict",
                    Detail = ex.Message,
                    Status = StatusCodes.Status409Conflict,
                    Type = "https://errors.yourapi.com/conflict.invalid_operation"
                }, code: "conflict.invalid_operation");
                return;

            // 401 — auth
            case InvalidCredentialsException:
                Write(ctx, new ProblemDetails
                {
                    Title = "Unauthorized",
                    Detail = ex.Message,
                    Status = StatusCodes.Status401Unauthorized,
                    Type = "https://errors.yourapi.com/auth.invalid_credentials"
                }, code: "auth.invalid_credentials");
                return;

            // 404 - not found
            case KeyNotFoundException:
                Write(ctx, new ProblemDetails
                {
                    Title = "Not Found",
                    Detail = ex.Message,
                    Status = StatusCodes.Status404NotFound,
                    Type = "https://errors.yourapi.com/user.not_found"
                }, code: "user.not_found");
                return;

            // 400 — bad request family
            case ArgumentNullException:
            case ArgumentOutOfRangeException:
            case ArgumentException:
                Write(ctx, new ProblemDetails
                {
                    Title = "Bad Request",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest,
                    Type = "https://errors.yourapi.com/request.bad_request"
                }, code: "request.bad_request");
                return;

            // 500 — fallback
            default:
                Write(ctx, new ProblemDetails
                {
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred.",
                    Status = StatusCodes.Status500InternalServerError,
                    Type = "https://errors.yourapi.com/internal"
                }, code: "internal.error");
                return;
        }
    }

    private static void Write(ExceptionContext ctx, ProblemDetails pd, string code)
    {
        pd.Extensions["code"] = code;
        ctx.Result = new ObjectResult(pd) { StatusCode = pd.Status };
        ctx.ExceptionHandled = true;
    }
}
