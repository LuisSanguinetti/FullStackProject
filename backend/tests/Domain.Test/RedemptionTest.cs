namespace Domain.Test;

[TestClass]
public class RedemptionTest
{
    [TestMethod]
    public void CreateRedemption_WithCtor_SetsAllFields()
    {
        // arrange
        var user = new User("Luis", "Test", "luis@example.com", "Password1", new DateOnly(2000, 1, 1), MembershipLevel.Premium);
        var reward = new Reward("Free Drink", "Redeem for one beverage.", 25);
        var when = new DateTime(2025, 9, 19, 12, 0, 0);
        var cost = 25;

        // act
        var r = new Redemption(user.Id, reward.Id, when, cost, reward, user);

        // assert
        Assert.AreNotEqual(Guid.Empty, r.Id);
        Assert.AreEqual(user.Id, r.UserId);
        Assert.AreEqual(reward.Id, r.RewardId);
        Assert.AreEqual(user, r.User);
        Assert.AreEqual(reward, r.Reward);
        Assert.AreEqual(when, r.DateClaimed);
        Assert.AreEqual(cost, r.CostPoints);
    }

    [TestMethod]
    public void Redemption_DefaultCtor_WithObjectInitializer_SetsMembers()
    {
        // arrange
        var id = Guid.NewGuid();
        var user = new User("Alice", "Baker", "alice@example.com", "Secret123!", new DateOnly(1999, 5, 5), MembershipLevel.Standard);
        var reward = new Reward("VIP Lounge Access", "Access for one day.", 100);
        var when = new DateTime(2025, 9, 20, 18, 0, 0);

        // act
        var r = new Redemption
        {
            Id = id,
            UserId = user.Id,
            User = user,
            RewardId = reward.Id,
            Reward = reward,
            DateClaimed = when,
            CostPoints = reward.CostPoints
        };

        // assert
        Assert.AreEqual(id, r.Id);
        Assert.AreEqual(user.Id, r.UserId);
        Assert.AreEqual(user, r.User);
        Assert.AreEqual(reward.Id, r.RewardId);
        Assert.AreEqual(reward, r.Reward);
        Assert.AreEqual(when, r.DateClaimed);
        Assert.AreEqual(100, r.CostPoints);
    }
}
