using Domain;
using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;

namespace Controller.Test;

[TestClass]
public class RedemptionControllerTest
{
    private Mock<IRedemptionLogic> _logic = null!;
    private RedemptionController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logic = new Mock<IRedemptionLogic>(MockBehavior.Strict);
        _controller = new RedemptionController(_logic.Object);
    }

    [TestMethod]
    public void Redeem_WithValidRewardId_CallsLogicAndReturnsOk()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var httpContext = new DefaultHttpContext();
        httpContext.Items["CurrentUserId"] = userId;
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        _logic.Setup(l => l.Redeem(userId, rewardId));

        // Act
        var action = _controller.Redeem(rewardId);

        // Assert
        var ok = action as OkResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);

        _logic.Verify(l => l.Redeem(userId, rewardId), Times.Once);
        _logic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetAllRedemptions_ReturnsOkWithList()
    {
        // Arrange
        var redemptions = new List<Redemption>
    {
        new Redemption
        {
            Id = Guid.NewGuid(),
            User = new User { Name = "User1", Email = "test1@test.com", Password = "pass1" },
            Reward = new Reward { Name = "Reward1", Description = "Desc1", CostPoints = 10, QuantityAvailable = 5 },
            CostPoints = 10,
            DateClaimed = DateTime.Now
        }
    };

        _logic.Setup(l => l.GetAllRedemptions()).Returns(redemptions);

        // Act
        var result = _controller.GetAll();

        // Assert
        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(200, ok.StatusCode);

        var returned = ok.Value as IEnumerable<Redemption>;
        Assert.IsNotNull(returned);
        Assert.AreEqual(redemptions.Count, returned.Count());

        _logic.Verify(l => l.GetAllRedemptions(), Times.Once);
        _logic.VerifyNoOtherCalls();
    }
}