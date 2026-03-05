namespace Domain.Test;

[TestClass]
public class MissionCompletionTest
{
    [TestMethod]
    public void CreateMissionCompletion_WithCtor_SetsAllFields()
    {
        // arrange
        var user = new User("Luis", "Test", "luis@example.com", "Password1", new DateOnly(2000, 1, 1), MembershipLevel.Premium);
        var mission = new Mission("Visit 3 Attractions", "Do three attractions today", 50);
        var when = new DateTime(2025, 9, 19, 12, 0, 0);
        var pts = 75;

        // act
        var mc = new MissionCompletion(user.Id, mission.Id, when, pts);

        // assert
        Assert.AreNotEqual(Guid.Empty, mc.Id);
        Assert.AreEqual(user.Id, mc.UserId);
        Assert.AreEqual(mission.Id, mc.MissionId);
        Assert.AreEqual(when, mc.DateCompleted);
        Assert.AreEqual(pts, mc.Points);
    }

    [TestMethod]
    public void MissionCompletion_DefaultCtor_WithObjectInitializer_SetsMembers()
    {
        // arrange
        var id = Guid.NewGuid();
        var user = new User("Alice", "Baker", "alice@example.com", "Secret123!", new DateOnly(1999, 5, 5), MembershipLevel.Standard);
        var mission = new Mission("Attend Event", "Join any special event", 20);
        var when = new DateTime(2025, 9, 20, 18, 0, 0);

        // act
        var mc = new MissionCompletion
        {
            Id = id,
            UserId = user.Id,
            User = user,
            MissionId = mission.Id,
            Mission = mission,
            DateCompleted = when,
            Points = 20
        };

        // assert
        Assert.AreEqual(id, mc.Id);
        Assert.AreEqual(user.Id, mc.UserId);
        Assert.AreEqual(user, mc.User);
        Assert.AreEqual(mission.Id, mc.MissionId);
        Assert.AreEqual(mission, mc.Mission);
        Assert.AreEqual(when, mc.DateCompleted);
        Assert.AreEqual(20, mc.Points);
    }
}
