using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic.Test;

[TestClass]
public class RedemptionLogicTest
{
    private Mock<IRepository<Redemption>> _repositoryRedemption = null!;
    private RedemptionLogic _redemptionLogic = null!;
    private Mock<IUserLogic> _users = null!;
    private Mock<IRewardLogic> _rewardLogic = null!;
    private Mock<ISessionLogic> _sessionLogic = null!;

    [TestInitialize]
    public void Setup()
    {
        _repositoryRedemption = new Mock<IRepository<Redemption>>(MockBehavior.Strict);
        _users = new Mock<IUserLogic>(MockBehavior.Strict);
        _rewardLogic = new Mock<IRewardLogic>(MockBehavior.Strict);
        _sessionLogic = new Mock<ISessionLogic>(MockBehavior.Strict);
        _redemptionLogic = new RedemptionLogic(_repositoryRedemption.Object, _users.Object, _rewardLogic.Object);
    }

    [TestMethod]
    public void RedeemTest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var rewardId = Guid.NewGuid();

        var reward = new Reward("Reward Test", "Desc", 100)
        {
            Id = rewardId,
            QuantityAvailable = 5
        };

        var user = new User("Facu", "Acuña", "f1@test.com", "1234",
            new DateOnly(2003, 5, 12), MembershipLevel.Standard)
        {
            Id = userId,
            Points = 500
        };

        Redemption? capturedRedemption = null;

        _rewardLogic
            .Setup(r => r.GetById(rewardId))
            .Returns(reward);

        _rewardLogic
            .Setup(r => r.DeductAvailable(rewardId));

        _users
            .Setup(u => u.GetByIdOrThrow(userId))
            .Returns(user);

        _users
            .Setup(u => u.DeductPoints(userId, reward.CostPoints));

        _repositoryRedemption
        .Setup(r => r.Add(It.IsAny<Redemption>()))
        .Callback<Redemption>(r => capturedRedemption = r)
        .Returns((Redemption r) => r);

        // Act
        _redemptionLogic.Redeem(userId, rewardId);

        // Assert
        _users.Verify(u => u.GetByIdOrThrow(userId), Times.Once);
        _rewardLogic.Verify(r => r.GetById(rewardId), Times.Once);
        _users.Verify(u => u.DeductPoints(userId, reward.CostPoints), Times.Once);
        _rewardLogic.Verify(r => r.DeductAvailable(rewardId), Times.Once);
        _repositoryRedemption.Verify(r => r.Add(It.IsAny<Redemption>()), Times.Once);

        Assert.IsNotNull(capturedRedemption);
        Assert.AreEqual(userId, capturedRedemption.UserId);
        Assert.AreEqual(rewardId, capturedRedemption.RewardId);
        Assert.AreEqual(reward.CostPoints, capturedRedemption.CostPoints);
    }

    [TestMethod]
    public void GetAllRedemptions()
    {
        // Arrange
        var reward1 = new Reward("Test1", "Desc", 100) { Id = Guid.NewGuid(), QuantityAvailable = 2 };
        var reward2 = new Reward("Test2", "Desc", 200) { Id = Guid.NewGuid(), QuantityAvailable = 3 };

        var user1 = new User("Facu", "Acuña", "f1@test.com", "1234", new DateOnly(2003, 5, 12), MembershipLevel.Standard)
        { Id = Guid.NewGuid(), Points = 500 };

        var user2 = new User("Pablo", "Acuña", "f2@test.com", "1234", new DateOnly(2003, 5, 12), MembershipLevel.Standard)
        { Id = Guid.NewGuid(), Points = 800 };

        var redemptions = new List<Redemption>
    {
        new Redemption(user1.Id, reward1.Id, DateTime.Now, reward1.CostPoints, reward1, user1),
        new Redemption(user2.Id, reward2.Id, DateTime.Now, reward2.CostPoints, reward2, user2)
    };

        _repositoryRedemption
            .Setup(r => r.FindAll())
            .Returns(redemptions);

        // Act
        var result = _redemptionLogic.GetAllRedemptions();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo(redemptions);

        _repositoryRedemption.Verify(r => r.FindAll(), Times.Once());
    }
}
