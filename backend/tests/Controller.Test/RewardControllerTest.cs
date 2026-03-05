using Domain;
using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;

[TestClass]
public class RewardControllerTest
{
    private Mock<IRewardLogic> _logicReward = null!;
    private RewardController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logicReward = new Mock<IRewardLogic>(MockBehavior.Strict);
        _controller = new RewardController(_logicReward.Object);
    }

    [TestMethod]
    public void CreateRewardTestController()
    {
        // Arrange
        var dto = new CreateRewardDto
        {
            Name = "Super Reward",
            Description = "Test reward",
            CostPoints = 100,
            QuantityAvailable = 50,
            MembershipLevel = null
        };

        var rewardMock = new Reward
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            CostPoints = dto.CostPoints,
            QuantityAvailable = dto.QuantityAvailable,
            MembershipLevel = dto.MembershipLevel
        };

        _logicReward
            .Setup(l => l.CreateReward(dto.Name, dto.Description, dto.CostPoints, dto.QuantityAvailable, dto.MembershipLevel))
            .Returns(rewardMock);

        // Act
        var action = _controller.Create(dto);

        // Assert
        var ok = action as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().Be(rewardMock);
        _logicReward.Verify(l => l.CreateReward(dto.Name, dto.Description, dto.CostPoints, dto.QuantityAvailable, dto.MembershipLevel), Times.Once);
        _logicReward.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetAllRewards()
    {
        // Arrange
        var rewards = new List<Reward>
    {
        new Reward { Id = Guid.NewGuid(), Name = "Reward 1", Description = "Desc 1", CostPoints = 100, QuantityAvailable = 10 },
        new Reward { Id = Guid.NewGuid(), Name = "Reward 2", Description = "Desc 2", CostPoints = 200, QuantityAvailable = 5 }
    };

        _logicReward
            .Setup(l => l.GetAllRewards())
            .Returns(rewards);

        // Act
        var action = _controller.GetAll();

        // Assert
        var ok = action as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeEquivalentTo(rewards);

        _logicReward.Verify(l => l.GetAllRewards(), Times.Once);
        _logicReward.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetRewardIdTest()
    {
        // Arrange
        var rewardId = Guid.NewGuid();

        var rewardMock = new Reward
        {
            Id = rewardId,
            Name = "Reward Test",
            Description = "Description test",
            CostPoints = 150,
            QuantityAvailable = 20,
            MembershipLevel = MembershipLevel.Standard
        };

        _logicReward
            .Setup(l => l.GetById(rewardId))
            .Returns(rewardMock);

        // Act
        var action = _controller.GetRewardId(rewardId);

        // Assert
        var ok = action as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().Be(rewardMock);

        _logicReward.Verify(l => l.GetById(rewardId), Times.Once);
        _logicReward.VerifyNoOtherCalls();
    }
}