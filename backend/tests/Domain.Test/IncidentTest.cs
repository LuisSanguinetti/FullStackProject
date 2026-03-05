namespace Domain.Test;

[TestClass]
public class IncidentTest
{
    [TestMethod]
    public void CreateIncident_WithCtor_SetsFields()
    {
        // arrange
        var attraction = new Attraction("Hyper Coaster", AttractionType.RollerCoaster, 12, 24, "Fast ride", basePoints: 10);
        var desc = "Engine fault";
        var reported = new DateTime(2025, 9, 19, 10, 0, 0);

        // act
        var inc = new Incident(desc, reported, attraction, attraction.Id);

        // assert
        Assert.AreNotEqual(Guid.Empty, inc.Id);
        Assert.AreEqual(attraction.Id, inc.AttractionId);
        Assert.AreEqual(attraction, inc.Attraction);
        Assert.AreEqual(desc, inc.Description);
        Assert.AreEqual(reported, inc.ReportedAt);
        Assert.IsFalse(inc.Resolved);
        Assert.AreEqual(default(DateTime), inc.ResolvedAt);
    }

    [TestMethod]
    public void Resolve_SetsResolvedAndTimestamp()
    {
        // arrange
        var attraction = new Attraction("Lab", AttractionType.InteractiveZone, 0, 50, "Hands-on", basePoints: 10);
        var reported = new DateTime(2025, 9, 19, 9, 0, 0);
        var inc = new Incident("Door jam", reported, attraction, attraction.Id);
        var when = new DateTime(2025, 9, 19, 9, 30, 0);

        // act
        inc.Resolve(when);

        // assert
        Assert.IsTrue(inc.Resolved);
        Assert.AreEqual(when, inc.ResolvedAt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Resolve_BeforeReportedAt_Throws()
    {
        // arrange
        var attraction = new Attraction("Simulator", AttractionType.Simulator, 8, 24, "4D", basePoints: 10);
        var reported = new DateTime(2025, 9, 19, 9, 0, 0);
        var inc = new Incident("Power issue", reported, attraction, attraction.Id);
        var before = new DateTime(2025, 9, 19, 8, 59, 59);

        // act
        inc.Resolve(before);

        // assert
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Resolve_Twice_Throws()
    {
        // arrange
        var attraction = new Attraction("Show", AttractionType.Show, 0, 200, "Lights", basePoints: 10);
        var inc = new Incident("Sensor error", new DateTime(2025, 9, 19, 9, 0, 0), attraction, attraction.Id);
        inc.Resolve(new DateTime(2025, 9, 19, 9, 5, 0));

        // act
        inc.Resolve(new DateTime(2025, 9, 19, 9, 10, 0));

        // assert
    }
}
