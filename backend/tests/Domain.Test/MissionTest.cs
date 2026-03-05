namespace Domain.Test;

[TestClass]
public class MissionTest
{
    [TestMethod]
    public void CreateMission_WithCtor_SetsAllFields()
    {
        // arrange
        var title = "Visit 3 Attractions";
        var desc = "Complete three different attractions in one day.";
        var points = 50;

        // act
        var m = new Mission(title, desc, points);

        // assert
        Assert.AreNotEqual(Guid.Empty, m.Id);
        Assert.AreEqual(title, m.Title);
        Assert.AreEqual(desc, m.Description);
        Assert.AreEqual(points, m.BasePoints);
    }

    [TestMethod]
    public void Mission_DefaultCtor_WithObjectInitializer_SetsRequiredMembers()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        var m = new Mission
        {
            Id = id,
            Title = "Attend Special Event",
            Description = "Participate in any special event.",
            BasePoints = 20
        };

        // assert
        Assert.AreEqual(id, m.Id);
        Assert.AreEqual("Attend Special Event", m.Title);
        Assert.AreEqual("Participate in any special event.", m.Description);
        Assert.AreEqual(20, m.BasePoints);
    }
}
