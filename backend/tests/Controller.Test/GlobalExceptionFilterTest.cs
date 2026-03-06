using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using obligatorio.WebApi.Filters;
using Park.BusinessLogic.Exceptions;

namespace Controller.Test;

[TestClass]
public class GlobalExceptionFilterTest
{
        private static ExceptionContext MakeCtx(Exception ex)
    {
        var http = new DefaultHttpContext();
        var actionContext = new ActionContext(http, new RouteData(), new ActionDescriptor());
        var ctx = new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = ex
        };
        return ctx;
    }

    [TestMethod]
    public void OnException_DuplicateEmail_Returns409_WithProblemDetailsAndCode()
    {
        // arrange
        var filter = new GlobalExceptionFilter();
        var ctx = MakeCtx(new DuplicateEmailException("dup@ex.com"));

        // act
        filter.OnException(ctx);

        // assert
        ctx.ExceptionHandled.Should().BeTrue();
        var obj = ctx.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var pd = obj.Value.Should().BeOfType<ProblemDetails>().Subject;
        pd.Title.Should().Be("Conflict");
        pd.Status.Should().Be(StatusCodes.Status409Conflict);
        pd.Detail.Should().Contain("dup@ex.com");
        pd.Type.Should().Be("https://errors.yourapi.com/user.email.duplicate");
        pd.Extensions.Should().ContainKey("code");
        pd.Extensions["code"].Should().Be("user.email.duplicate");
    }

    [TestMethod]
    public void OnException_InvalidCredentials_Returns401_WithProblemDetailsAndCode()
    {
        // arrange
        var filter = new GlobalExceptionFilter();
        var ctx = MakeCtx(new InvalidCredentialsException());

        // act
        filter.OnException(ctx);

        // assert
        ctx.ExceptionHandled.Should().BeTrue();
        var obj = ctx.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);

        var pd = obj.Value.Should().BeOfType<ProblemDetails>().Subject;
        pd.Title.Should().Be("Unauthorized");
        pd.Status.Should().Be(StatusCodes.Status401Unauthorized);
        pd.Detail.Should().Be("Email or password is incorrect.");
        pd.Type.Should().Be("https://errors.yourapi.com/auth.invalid_credentials");
        pd.Extensions.Should().ContainKey("code");
        pd.Extensions["code"].Should().Be("auth.invalid_credentials");
    }

    [TestMethod]
    public void OnException_KeyNotFound_Returns404_WithProblemDetailsAndCode()
    {
        // arrange
        var filter = new GlobalExceptionFilter();
        var ctx = MakeCtx(new KeyNotFoundException("User not found"));

        // act
        filter.OnException(ctx);

        // assert
        ctx.ExceptionHandled.Should().BeTrue();
        var obj = ctx.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status404NotFound);

        var pd = obj.Value.Should().BeOfType<ProblemDetails>().Subject;
        pd.Title.Should().Be("Not Found");
        pd.Status.Should().Be(StatusCodes.Status404NotFound);
        pd.Detail.Should().Contain("not found");
        pd.Type.Should().Be("https://errors.yourapi.com/user.not_found");
        pd.Extensions.Should().ContainKey("code");
        pd.Extensions["code"].Should().Be("user.not_found");
    }

    [TestMethod]
    public void OnException_Other_Returns500_FallbackProblemDetails()
    {
        // arrange
        var filter = new GlobalExceptionFilter();
        var ctx = MakeCtx(new Exception("boom"));

        // act
        filter.OnException(ctx);

        // assert
        ctx.ExceptionHandled.Should().BeTrue();
        var obj = ctx.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);

        var pd = obj.Value.Should().BeOfType<ProblemDetails>().Subject;
        pd.Title.Should().Be("Internal Server Error");
        pd.Status.Should().Be(StatusCodes.Status500InternalServerError);
        pd.Detail.Should().Be("An unexpected error occurred.");
    }

        [TestMethod]
    public void OnException_InvalidOperation_Returns409_WithProblemDetailsAndCode()
    {
        // arrange
        var filter = new GlobalExceptionFilter();
        var ctx = MakeCtx(new InvalidOperationException("invalid op"));

        // act
        filter.OnException(ctx);

        // assert
        ctx.ExceptionHandled.Should().BeTrue();
        var obj = ctx.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var pd = obj.Value.Should().BeOfType<ProblemDetails>().Subject;
        pd.Title.Should().Be("Conflict");
        pd.Status.Should().Be(StatusCodes.Status409Conflict);
        pd.Detail.Should().Be("invalid op");
        pd.Type.Should().Be("https://errors.yourapi.com/conflict.invalid_operation");
        pd.Extensions["code"].Should().Be("conflict.invalid_operation");
    }

    [TestMethod]
    public void OnException_ArgumentException_Returns400_WithProblemDetailsAndCode()
    {
        // arrange
        var filter = new GlobalExceptionFilter();
        var ctx = MakeCtx(new ArgumentException("bad arg", "x"));

        // act
        filter.OnException(ctx);

        // assert
        ctx.ExceptionHandled.Should().BeTrue();
        var obj = ctx.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var pd = obj.Value.Should().BeOfType<ProblemDetails>().Subject;
        pd.Title.Should().Be("Bad Request");
        pd.Status.Should().Be(StatusCodes.Status400BadRequest);
        pd.Detail.Should().Contain("bad arg");
        pd.Type.Should().Be("https://errors.yourapi.com/request.bad_request");
        pd.Extensions["code"].Should().Be("request.bad_request");
    }

    [TestMethod]
    public void OnException_ArgumentOutOfRange_Returns400_WithProblemDetailsAndCode()
    {
        // arrange
        var filter = new GlobalExceptionFilter();
        var ctx = MakeCtx(new ArgumentOutOfRangeException("cap", "oops"));

        // act
        filter.OnException(ctx);

        // assert
        ctx.ExceptionHandled.Should().BeTrue();
        var obj = ctx.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var pd = obj.Value.Should().BeOfType<ProblemDetails>().Subject;
        pd.Title.Should().Be("Bad Request");
        pd.Status.Should().Be(StatusCodes.Status400BadRequest);
        pd.Detail.Should().Contain("oops");
        pd.Type.Should().Be("https://errors.yourapi.com/request.bad_request");
        pd.Extensions["code"].Should().Be("request.bad_request");
    }

    [TestMethod]
    public void OnException_ArgumentNull_Returns400_WithProblemDetailsAndCode()
    {
        // arrange
        var filter = new GlobalExceptionFilter();
        var ctx = MakeCtx(new ArgumentNullException("name", "is null"));

        // act
        filter.OnException(ctx);

        // assert
        ctx.ExceptionHandled.Should().BeTrue();
        var obj = ctx.Result.Should().BeOfType<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        var pd = obj.Value.Should().BeOfType<ProblemDetails>().Subject;
        pd.Title.Should().Be("Bad Request");
        pd.Status.Should().Be(StatusCodes.Status400BadRequest);
        pd.Detail.Should().Contain("is null");
        pd.Type.Should().Be("https://errors.yourapi.com/request.bad_request");
        pd.Extensions["code"].Should().Be("request.bad_request");
    }
}
