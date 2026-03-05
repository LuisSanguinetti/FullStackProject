using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic.Test;

[TestClass]
public class RewardLogicTest
{
    private Mock<IRepository<Reward>> _rewardRepo = null!;
    private RewardLogic _rewardLogic = null!;

    [TestInitialize]
    public void Setup()
    {
        _rewardRepo = new Mock<IRepository<Reward>>(MockBehavior.Strict);
        _rewardLogic = new RewardLogic(_rewardRepo.Object);
    }

    [TestMethod]
    public void CreateRewardTest()
    {
        // Arrange
        var name = "Test reward";
        var description = "Test";
        var costPoints = 100;
        var quantity = 50;

        Reward? capturedReward = null;

        _rewardRepo
     .Setup(r => r.Add(It.IsAny<Reward>()))
     .Callback<Reward>(r => capturedReward = r)
     .Returns((Reward r) => r);

        // Act
        _rewardLogic.CreateReward(name, description, costPoints, quantity);

        // Assert
        _rewardRepo.Verify(r => r.Add(It.IsAny<Reward>()), Times.Once);
        Assert.IsNotNull(capturedReward);
        Assert.AreEqual(name, capturedReward.Name);
        Assert.AreEqual(description, capturedReward.Description);
        Assert.AreEqual(costPoints, capturedReward.CostPoints);
        Assert.AreEqual(quantity, capturedReward.QuantityAvailable);
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

        _rewardRepo
            .Setup(r => r.FindAll(It.IsAny<Expression<Func<Reward, object>>[]>()))
            .Returns(rewards);

        // Act
        var result = _rewardLogic.GetAllRewards();

        // Assert
        _rewardRepo.Verify(r => r.FindAll(It.IsAny<Expression<Func<Reward, object>>[]>()), Times.Once);
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(rewards);
    }

    [TestMethod]
    public void GetByIdTest()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var reward = new Reward
        {
            Id = rewardId,
            Name = "Reward Test",
            Description = "Desc",
            CostPoints = 50,
            QuantityAvailable = 20
        };

        _rewardRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<Reward, bool>>>()))
            .Returns(reward);

        // Act
        var result = _rewardLogic.GetById(rewardId);

        // Assert
        _rewardRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Reward, bool>>>()), Times.Once);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(reward);
    }

    [TestMethod]
    public void DeductAvailableTest()
    {
        // Arrange
        var rewardId = Guid.NewGuid();
        var reward = new Reward
        {
            Id = rewardId,
            Name = "Reward Deduct",
            Description = "Desc",
            CostPoints = 100,
            QuantityAvailable = 10
        };

        _rewardRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<Reward, bool>>>()))
            .Returns(reward);

        _rewardRepo
            .Setup(r => r.Update(It.IsAny<Reward>()))
            .Returns((Reward r) => r);

        // Act
        _rewardLogic.DeductAvailable(rewardId);

        // Assert
        _rewardRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Reward, bool>>>()), Times.Once);
        _rewardRepo.Verify(r => r.Update(It.IsAny<Reward>()), Times.Once);
        Assert.AreEqual(9, reward.QuantityAvailable);
    }
}