using Domain;
using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;
using Park.BusinessLogic.Exceptions;

namespace Controller.Test;

[TestClass]
public class CustomDateTimeProviderControllerTest
{
    private Mock<ICustomDateTimeProvider> _logicMock = null!;
    private CustomDateTimeProviderController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logicMock = new Mock<ICustomDateTimeProvider>(MockBehavior.Strict);
        _controller = new CustomDateTimeProviderController(_logicMock.Object);
    }

    [TestMethod]
    public void PutSetTimeTest()
    {
        // Arrange
        var customTime = new DateTime(2025, 10, 6, 12, 0, 0);
        _logicMock.Setup(l => l.SetCustomTime(customTime));

        // Act
        var result = _controller.PutSetTime(customTime);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual("time updated", okResult.Value);
        _logicMock.Verify(l => l.SetCustomTime(customTime), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetCurrentTimeTest()
    {
        // Arrange
        var expectedTime = new DateTime(2025, 10, 6, 15, 30, 0, DateTimeKind.Utc);
        _logicMock.Setup(l => l.GetNowUtc()).Returns(expectedTime);

        // Act
        var result = _controller.GetCurrentTime();

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedTime, okResult.Value);

        _logicMock.Verify(l => l.GetNowUtc(), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }
}
