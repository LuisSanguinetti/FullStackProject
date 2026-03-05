using System;
using System.Security.Authentication;
using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using obligatorio.WebApi.Controllers;

namespace Controller.Test;

[TestClass]
public class MissionCompletionControllerTest
{
    private Mock<IMissionCompletionLogic> _logic = null!;
    private MissionCompletionController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logic = new Mock<IMissionCompletionLogic>(MockBehavior.Strict);
        _controller = new MissionCompletionController(_logic.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [TestMethod]
    public void Create_Returns_Ok_With_Points_And_Calls_Register_With_UserId_And_MissionId()
    {
        // arrange
        var missionId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var scoringStrategyId = Guid.NewGuid();
        const int points = 37;

        _logic.Setup(l => l.Register(userId, missionId, It.IsAny<DateTime>(), scoringStrategyId))
            .Returns(points);

        // simulate AuthAttribute putting the user in HttpContext.Items
        _controller.HttpContext.Items["CurrentUserId"] = userId;

        // act
        var result = _controller.Create(missionId, scoringStrategyId);

        // assert
        result.Should().BeOfType<OkObjectResult>();
        var ok = (OkObjectResult)result;
        ok.Value.Should().Be(points);

        _logic.Verify(l => l.Register(userId, missionId, It.IsAny<DateTime>(), scoringStrategyId), Times.Once);
        _logic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Create_Throws_InvalidCredential_When_UserId_Missing()
    {
        // arrange
        var missionId = Guid.NewGuid();
        var scoringStrategyId = Guid.NewGuid();

        // act
        Action act = () => _controller.Create(missionId, scoringStrategyId);

        // assert
        act.Should().Throw<InvalidCredentialException>();
        _logic.VerifyNoOtherCalls();
    }
}
