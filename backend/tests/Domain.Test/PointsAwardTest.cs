namespace Domain.Test;

[TestClass]
public class PointsAwardTest
{
    [TestMethod]
    public void CreatePointsAward_WithCtor_SetsAllFields()
    {
        // arrange
        var user = new User("Luis", "Test", "luis@example.com", "Password1", new DateOnly(2000, 1, 1), MembershipLevel.Premium);
        var pts = 40;
        var reason = $"access:{Guid.NewGuid()}";
        var strategyId = Guid.NewGuid();
        var when = new DateTime(2025, 9, 19, 12, 0, 0);

        // act
        var pa = new PointsAward(user.Id, user, pts, reason, strategyId, when);

        // assert
        Assert.AreNotEqual(Guid.Empty, pa.Id);
        Assert.AreEqual(user.Id, pa.UserId);
        Assert.AreEqual(user, pa.User);
        Assert.AreEqual(pts, pa.Points);
        Assert.AreEqual(reason, pa.Reason);
        Assert.AreEqual(strategyId, pa.StrategyId);
        Assert.AreEqual(when, pa.At);
    }

    [TestMethod]
    public void PointsAward_DefaultCtor_WithObjectInitializer_SetsMembers()
    {
        // arrange
        var id = Guid.NewGuid();
        var user = new User("Alice", "Baker", "alice@example.com", "Secret123!", new DateOnly(1999, 5, 5), MembershipLevel.Standard);
        var reason = "mission:abc123";
        var strategyId = Guid.NewGuid();
        var when = new DateTime(2025, 9, 20, 9, 30, 0);

        // act
        var pa = new PointsAward
        {
            Id = id,
            UserId = user.Id,
            User = user,
            Points = 10,
            Reason = reason,
            StrategyId = strategyId,
            At = when
        };

        // assert
        Assert.AreEqual(id, pa.Id);
        Assert.AreEqual(user.Id, pa.UserId);
        Assert.AreEqual(user, pa.User);
        Assert.AreEqual(10, pa.Points);
        Assert.AreEqual(reason, pa.Reason);
        Assert.AreEqual(strategyId, pa.StrategyId);
        Assert.AreEqual(when, pa.At);
    }
}
