namespace Domain.Test;

[TestClass]
public class TicketTest
{
[TestMethod]
    public void CreateTicket_WithCtor_SetsAllFields()
    {
        // arrange
        var qr = Guid.NewGuid();
        var visit = new DateOnly(2025, 9, 20);
        var owner = new User("Luis", "Test", "luis@example.com", "Password1", new DateOnly(2000, 1, 1), MembershipLevel.Premium);
        var ev = new SpecialEvent("Music Fest",
                                  new DateTime(2025, 11, 1, 18, 0, 0),
                                  new DateTime(2025, 11, 1, 23, 59, 0),
                                  500, 10.0m, null);

        // act
        var t = new Ticket(qr, visit, TicketType.SpecialEvent, owner, owner.Id, ev, ev.Id);

        // assert
        Assert.AreNotEqual(Guid.Empty, t.Id);
        Assert.AreEqual(qr, t.QrCode);
        Assert.AreEqual(visit, t.VisitDate);
        Assert.AreEqual(TicketType.SpecialEvent, t.Type);
        Assert.AreEqual(owner, t.Owner);
        Assert.AreEqual(owner.Id, t.UserId);
        Assert.AreEqual(ev, t.SpecialEvent);
        Assert.AreEqual(ev.Id, t.SpecialEventId);
    }

    [TestMethod]
    public void Ticket_DefaultCtor_WithObjectInitializer_General_NoEvent()
    {
        // arrange
        var id = Guid.NewGuid();
        var qr = Guid.NewGuid();
        var visit = new DateOnly(2025, 9, 21);
        var owner = new User("Alice", "Baker", "alice@example.com", "Secret123!", new DateOnly(1999, 5, 5), MembershipLevel.Standard);

        // act
        var t = new Ticket
        {
            Id = id,
            QrCode = qr,
            VisitDate = visit,
            Type = TicketType.General,
            Owner = owner,
            UserId = owner.Id,
            SpecialEvent = null,
            SpecialEventId = null
        };

        // assert
        Assert.AreEqual(id, t.Id);
        Assert.AreEqual(qr, t.QrCode);
        Assert.AreEqual(visit, t.VisitDate);
        Assert.AreEqual(TicketType.General, t.Type);
        Assert.AreEqual(owner, t.Owner);
        Assert.AreEqual(owner.Id, t.UserId);
        Assert.IsNull(t.SpecialEvent);
        Assert.IsNull(t.SpecialEventId);
    }

    [TestMethod]
    public void CreateTicket_WithCtor_EventProvided_SetsEventFields()
    {
        // arrange
        var qr = Guid.NewGuid();
        var visit = new DateOnly(2025, 12, 24);
        var owner = new User("Bob", "Carson", "bob@example.com", "TopSecret!", new DateOnly(1995, 7, 7), MembershipLevel.Premium);
        var ev = new SpecialEvent("Xmas Gala",
                                  new DateTime(2025, 12, 24, 18, 0, 0),
                                  new DateTime(2025, 12, 24, 23, 0, 0),
                                  300, 12.5m, null);

        // act
        var t = new Ticket(qr, visit, TicketType.SpecialEvent, owner, owner.Id, ev, ev.Id);

        // assert
        Assert.AreEqual(ev, t.SpecialEvent);
        Assert.AreEqual(ev.Id, t.SpecialEventId);
    }
}
