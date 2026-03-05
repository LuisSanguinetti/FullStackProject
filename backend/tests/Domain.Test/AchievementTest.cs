namespace Domain.Test;

[TestClass]
public class AchievementTest
{
    [TestMethod]
    public void CreateAchievement_WithCtor_SetsAllFields()
    {
        // arrange
        var name = "First Ride";
        var desc = "Complete your first attraction.";

        // act
        var a = new Achievement(name, desc);

        // assert
        Assert.AreNotEqual(Guid.Empty, a.Id);
        Assert.AreEqual(name, a.Name);
        Assert.AreEqual(desc, a.Description);
    }

    [TestMethod]
    public void Achievement_DefaultCtor_WithObjectInitializer_SetsRequiredMembers()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        var a = new Achievement
        {
            Id = id,
            Name = "Early Bird",
            Description = "Enter the park before 9:00 AM."
        };

        // assert
        Assert.AreEqual(id, a.Id);
        Assert.AreEqual("Early Bird", a.Name);
        Assert.AreEqual("Enter the park before 9:00 AM.", a.Description);
    }
}
