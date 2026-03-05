namespace Domain.Test;

[TestClass]
public class UnlockLogTest
{
    [TestMethod]
    public void CreateUnlockLog_WithCtor_SetsAllFields()
    {
        // arrange
        var user = new User("Luis", "Test", "luis@example.com", "Password1", new DateOnly(2000, 1, 1), MembershipLevel.Premium);
        var ach = new Achievement("First Ride", "Complete your first attraction.");
        var when = new DateTime(2025, 9, 19, 12, 0, 0);

        // act
        var ul = new UnlockLog(user.Id, ach.Id, when, user, ach);

        // assert
        Assert.AreNotEqual(Guid.Empty, ul.Id);
        Assert.AreEqual(user.Id, ul.UserId);
        Assert.AreEqual(ach.Id, ul.AchievementId);
        Assert.AreEqual(user, ul.User);
        Assert.AreEqual(ach, ul.Achievement);
        Assert.AreEqual(when, ul.DateUnlocked);
    }

    [TestMethod]
    public void UnlockLog_DefaultCtor_WithObjectInitializer_SetsMembers()
    {
        // arrange
        var id = Guid.NewGuid();
        var user = new User("Alice", "Baker", "alice@example.com", "Secret123!", new DateOnly(1999, 5, 5), MembershipLevel.Standard);
        var ach = new Achievement("Early Bird", "Enter the park before 9 AM.");
        var when = new DateTime(2025, 9, 20, 8, 30, 0);

        // act
        var ul = new UnlockLog
        {
            Id = id,
            UserId = user.Id,
            User = user,
            AchievementId = ach.Id,
            Achievement = ach,
            DateUnlocked = when
        };

        // assert
        Assert.AreEqual(id, ul.Id);
        Assert.AreEqual(user.Id, ul.UserId);
        Assert.AreEqual(user, ul.User);
        Assert.AreEqual(ach.Id, ul.AchievementId);
        Assert.AreEqual(ach, ul.Achievement);
        Assert.AreEqual(when, ul.DateUnlocked);
    }
}
