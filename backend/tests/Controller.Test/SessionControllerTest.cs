using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;

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
}
