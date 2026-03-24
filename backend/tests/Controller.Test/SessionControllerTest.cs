using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;
[TestClass]
public class SessionControllerTest
{
    private Mock<IUserLogic> _logicMock = null!;
    private Mock<IUserRoleLogic> _userRoleLogicMock = null!;
    private Mock<ISessionLogic> _sessionLogicMock = null!;
    private SessionController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logicMock = new Mock<IUserLogic>(MockBehavior.Strict);
        _userRoleLogicMock = new Mock<IUserRoleLogic>(MockBehavior.Strict);
        _sessionLogicMock = new Mock<ISessionLogic>(MockBehavior.Strict);
        _controller = new SessionController(_sessionLogicMock.Object);
    }

    [TestMethod]
    public void Logout_WithValidBearerToken_CallsSessionLogout_AndReturns200()
    {
        // Arrange
        var token = Guid.NewGuid();
        _sessionLogicMock.Setup(s => s.DeleteSession(token));

        // Act
        var result = _controller.Logout($"Bearer {token}");

        // Assert
        result.Should().Match<IActionResult>(r => r is OkResult || r is OkObjectResult);
        if (result is OkObjectResult ok)
        {
            ok.Value.Should().BeEquivalentTo(new { message = "logged out" });
        }

        _sessionLogicMock.Verify(s => s.DeleteSession(token), Times.Once);
        _sessionLogicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Logout_WithRawToken_CallsSessionLogout_AndReturns200()
    {
        // Arrange
        var token = Guid.NewGuid();
        _sessionLogicMock.Setup(s => s.DeleteSession(token));

        // Act
        var result = _controller.Logout(token.ToString());

        // Assert
        result.Should().Match<IActionResult>(r => r is OkResult || r is OkObjectResult);
        if (result is OkObjectResult ok)
        {
            ok.Value.Should().BeEquivalentTo(new { message = "logged out" });
        }

        _sessionLogicMock.Verify(s => s.DeleteSession(token), Times.Once);
        _sessionLogicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void LogIn_Returns200_WithToken()
    {
        // Arrange
        var expected = Guid.NewGuid();
        var dto = new UserLogInDto { Email = "alice@ex.com", Password = "secret!" };

        _sessionLogicMock
            .Setup(l => l.Login("alice@ex.com", "secret!"))
            .Returns(expected);

        // Act
        var action = _controller.Login(dto);

        // Assert
        var result = action as OkObjectResult;
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        result.Value.Should().BeEquivalentTo(new { token = expected });

        _sessionLogicMock.Verify(l => l.Login("alice@ex.com", "secret!"), Times.Once);
    }

    [TestMethod]
    public void LogIn_PropagatesException_WhenCredentialsInvalid()
    {
        // Arrange
        var dto = new UserLogInDto { Email = "bad@ex.com", Password = "nope" };

        _sessionLogicMock
            .Setup(l => l.Login("bad@ex.com", "nope"))
            .Throws(new InvalidOperationException("Email or password is incorrect."));

        // Act
        Action act = () => _controller.Login(dto);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Email or password is incorrect.");

        _sessionLogicMock.Verify(l => l.Login("bad@ex.com", "nope"), Times.Once);
        _sessionLogicMock.VerifyNoOtherCalls();
    }
}
