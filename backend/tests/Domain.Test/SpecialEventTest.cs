namespace Domain.Test;

[TestClass]
public class SpecialEventTest
{
    [TestMethod]
    public void CreateSpecialEvent_WithCtor_SetsAllFields()
    {
        // Arrange
        var name = "Halloween Night";
        var start = new DateTime(2025, 10, 31, 18, 0, 0);
        var end = new DateTime(2025, 10, 31, 23, 59, 0);
        var capacity = 500;
        var extra = 15.99m;
        var attractions = new List<Attraction>
        {
            new Attraction("Haunted House", AttractionType.InteractiveZone, 10, 40, "Scary fun", basePoints: 10)
        };

        // Act
        var ev = new SpecialEvent(name, start, end, capacity, extra, attractions);

        // Assert
        Assert.IsNotNull(ev);
        Assert.AreNotEqual(Guid.Empty, ev.Id);
        Assert.AreEqual(name, ev.Name);
        Assert.AreEqual(start, ev.StartDate);
        Assert.AreEqual(end, ev.EndDate);
        Assert.AreEqual(capacity, ev.Capacity);
        Assert.AreEqual(extra, ev.ExtraPrice, 1e-9m);
        Assert.IsNotNull(ev.Attractions);
        Assert.AreEqual(1, ev.Attractions.Count);
        Assert.AreEqual("Haunted House", ev.Attractions[0].Name);
    }

    [TestMethod]
    public void CreateSpecialEvent_WithCtor_NullAttractions_LeavesEmptyList()
    {
        // Arrange
        var ev = new SpecialEvent("Summer Fest",
                                  new DateTime(2025, 12, 1, 10, 0, 0),
                                  new DateTime(2025, 12, 1, 20, 0, 0),
                                  300, 5.0m, null);

        // Assert
        Assert.IsNotNull(ev.Attractions);
        Assert.AreEqual(0, ev.Attractions.Count);
    }

    [TestMethod]
    public void CreateSpecialEvent_WithCtor_CopiesAttractions()
    {
        // Arrange
        var source = new List<Attraction>
        {
            new Attraction("Laser Show", AttractionType.Show, 0, 200, "Lights and music", basePoints: 10),
            new Attraction("4D Simulator", AttractionType.Simulator, 8, 24, "Immersive ride", basePoints: 10)
        };

        // Act
        var ev = new SpecialEvent("Tech Expo",
                                  new DateTime(2025, 9, 20, 9, 0, 0),
                                  new DateTime(2025, 9, 20, 17, 0, 0),
                                  800, 0.0m, source);

        source.Add(new Attraction("New Ride", AttractionType.RollerCoaster, 12, 28, "Added later", basePoints: 10));

        // Assert
        Assert.AreEqual(2, ev.Attractions.Count);
        Assert.AreEqual("Laser Show", ev.Attractions[0].Name);
        Assert.AreEqual("4D Simulator", ev.Attractions[1].Name);
    }

    [TestMethod]
    public void SpecialEvent_DefaultCtor_WithObjectInitializer_SetsRequiredMembers()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var ev = new SpecialEvent
        {
            Id = id,
            Name = "Music Marathon",
            StartDate = new DateTime(2025, 11, 15, 12, 0, 0),
            EndDate = new DateTime(2025, 11, 15, 23, 0, 0),
            Capacity = 1000,
            ExtraPrice = 9.5m,
            Attractions = new List<Attraction>
            {
                new Attraction("Main Stage", AttractionType.Show, 0, 500, "Headliners", basePoints: 10)
            }
        };

        // Assert
        Assert.AreEqual(id, ev.Id);
        Assert.AreEqual("Music Marathon", ev.Name);
        Assert.AreEqual(new DateTime(2025, 11, 15, 12, 0, 0), ev.StartDate);
        Assert.AreEqual(new DateTime(2025, 11, 15, 23, 0, 0), ev.EndDate);
        Assert.AreEqual(1000, ev.Capacity);
        Assert.AreEqual(9.5m, ev.ExtraPrice, 1e-9m);
        Assert.IsNotNull(ev.Attractions);
        Assert.AreEqual(1, ev.Attractions.Count);
        Assert.AreEqual("Main Stage", ev.Attractions[0].Name);
    }

    [TestMethod]
    public void Attractions_InitializedEmpty_ByDefault()
    {
        var ev = new SpecialEvent
        {
            Name = "No Attractions Yet",
            StartDate = new DateTime(2025, 9, 19, 10, 0, 0),
            EndDate = new DateTime(2025, 9, 19, 12, 0, 0),
            Capacity = 50,
            ExtraPrice = 0.0m
        };

        Assert.IsNotNull(ev.Attractions);
        Assert.AreEqual(0, ev.Attractions.Count);
    }
}
