namespace Domain.Test;

[TestClass]
public class AttractionTest
{
    [TestMethod]
    public void CreateAttraction_WithCtor_SetsAllFields()
    {
        // Arrange
        var name = "Hyper Coaster";
        var type = AttractionType.RollerCoaster;
        var minAge = 12;
        var maxCapacity = 24;
        var description = "High-speed coaster with inversions.";

        // Act
        var attraction = new Attraction(name, type, minAge, maxCapacity, description, basePoints: 10);

        // Assert
        Assert.IsNotNull(attraction);
        Assert.AreNotEqual(Guid.Empty, attraction.Id);
        Assert.AreEqual(name, attraction.Name);
        Assert.AreEqual(type, attraction.Type);
        Assert.AreEqual(minAge, attraction.MinAge);
        Assert.AreEqual(maxCapacity, attraction.MaxCapacity);
        Assert.AreEqual(description, attraction.Description);
    }

    [TestMethod]
    public void Attraction_DefaultCtor_WithObjectInitializer_SetsRequiredMembers()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var attraction = new Attraction
        {
            Id = id,
            Name = "Interactive Lab",
            Type = AttractionType.InteractiveZone,
            MinAge = 0,
            MaxCapacity = 50,
            Description = "Hands-on science exhibits."
        };

        // Assert
        Assert.AreEqual(id, attraction.Id);
        Assert.AreEqual("Interactive Lab", attraction.Name);
        Assert.AreEqual(AttractionType.InteractiveZone, attraction.Type);
        Assert.AreEqual(0, attraction.MinAge);
        Assert.AreEqual(50, attraction.MaxCapacity);
        Assert.AreEqual("Hands-on science exhibits.", attraction.Description);
    }

    [TestMethod]
    public void NewAttraction_IsEnabledByDefault()
    {
        var a = new Domain.Attraction("Coaster", Domain.AttractionType.RollerCoaster, 12, 24, "desc", 10);
        Assert.IsTrue(a.Enabled);
    }
}
