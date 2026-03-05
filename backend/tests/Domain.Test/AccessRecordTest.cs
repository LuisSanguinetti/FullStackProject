namespace Domain.Test;

[TestClass]
public class AccessRecordTest
{
        [TestMethod]
    public void CreateAccessRecord_WithCtor_SetsAllFields_WithEvent()
    {
        // arrange
        var owner = new User("Luis", "Test", "luis@example.com", "Password1", new DateOnly(2000, 1, 1), MembershipLevel.Premium);
        var ticket = new Ticket(Guid.NewGuid(), new DateOnly(2025, 9, 20), TicketType.SpecialEvent, owner, owner.Id,
                                new SpecialEvent("Music Fest",
                                    new DateTime(2025, 11, 1, 18, 0, 0),
                                    new DateTime(2025, 11, 1, 23, 59, 0),
                                    500, 10.0m, null),
                                null);
        var attraction = new Attraction("Hyper Coaster", AttractionType.RollerCoaster, 12, 24, "Fast ride", basePoints: 10);
        var ev = new SpecialEvent("Music Fest",
                                  new DateTime(2025, 11, 1, 18, 0, 0),
                                  new DateTime(2025, 11, 1, 23, 59, 0),
                                  500, 10.0m, null);
        var inAt = new DateTime(2025, 9, 20, 14, 0, 0);

        // act
        var ar = new AccessRecord(inAt,
                                  ticket, ticket.Id,
                                  attraction, attraction.Id, 0,
                                  ev, ev.Id);

        // assert
        Assert.AreNotEqual(Guid.Empty, ar.Id);
        Assert.AreEqual(inAt, ar.InAt);
        Assert.IsNull(ar.OutAt);
        Assert.AreEqual(ticket, ar.Ticket);
        Assert.AreEqual(ticket.Id, ar.TicketId);
        Assert.AreEqual(attraction, ar.Attraction);
        Assert.AreEqual(attraction.Id, ar.AttractionId);
        Assert.AreEqual(ev, ar.SpecialEvent);
        Assert.AreEqual(ev.Id, ar.SpecialEventId);
    }

    [TestMethod]
    public void CheckOut_SetsOutAt()
    {
        // arrange
        var owner = new User("Alice", "Baker", "alice@example.com", "Secret123!", new DateOnly(1999, 5, 5), MembershipLevel.Standard);
        var ticket = new Ticket(Guid.NewGuid(), new DateOnly(2025, 9, 21), TicketType.General, owner, owner.Id, null!, null);
        var attraction = new Attraction("Interactive Lab", AttractionType.InteractiveZone, 0, 50, "Hands-on", basePoints: 10);
        var inAt = new DateTime(2025, 9, 21, 10, 0, 0);
        var ar = new AccessRecord(inAt, ticket, ticket.Id, attraction,  attraction.Id, 0);
        var outAt = new DateTime(2025, 9, 21, 11, 30, 0);

        // act
        ar.CheckOut(outAt);

        // assert
        Assert.AreEqual(outAt, ar.OutAt);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void CheckOut_BeforeInAt_Throws()
    {
        // arrange
        var owner = new User("Bob", "Carson", "bob@example.com", "TopSecret!", new DateOnly(1995, 7, 7), MembershipLevel.Premium);
        var ticket = new Ticket(Guid.NewGuid(), new DateOnly(2025, 12, 24), TicketType.General, owner, owner.Id, null!, null);
        var attraction = new Attraction("4D Simulator", AttractionType.Simulator, 8, 24, "Immersive ride", basePoints: 10);
        var inAt = new DateTime(2025, 12, 24, 15, 0, 0);
        var ar = new AccessRecord(inAt, ticket, ticket.Id, attraction, attraction.Id, 0);
        var before = new DateTime(2025, 12, 24, 14, 59, 59);

        // act
        ar.CheckOut(before);

        // assert
    }
}
