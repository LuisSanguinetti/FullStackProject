using System;
using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class TicketLogicTest
{
    private Mock<IRepository<Ticket>> _repo = null!;
    private Mock<ISpecialEventLogic> _events = null!;
    private Mock<IUserLogic> _user = null!;
    private TicketLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new Mock<IRepository<Ticket>>(MockBehavior.Strict);
        _events = new Mock<ISpecialEventLogic>(MockBehavior.Strict);
        _user = new Mock<IUserLogic>(MockBehavior.Strict);
        _logic = new TicketLogic(_repo.Object, _events.Object, _user.Object);
    }

    [TestMethod]
    public void GetByQrOrThrow_Returns_Ticket_When_Found()
    {
        var qr = Guid.NewGuid();
        var t = new Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = qr,
            VisitDate = DateOnly.FromDateTime(dateTime: DateTime.Now),
            Type = TicketType.General,
            UserId = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"),
            Owner = new User("Alice", "Baker", "a@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard)
            {
                Id = Guid.Parse("AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA")
            }
        };

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Ticket, bool>>>()))
            .Returns((Expression<Func<Ticket, bool>> pred) =>
                pred.Compile()(t) ? t : null);

        var result = _logic.GetByQrOrThrow(qr);

        result.Should().BeSameAs(t);
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<Ticket, bool>>>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetByQrOrThrow_Throws_When_NotFound()
    {
        var qr = Guid.NewGuid();

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Ticket, bool>>>()))
            .Returns((Ticket?)null);

        Action act = () => _logic.GetByQrOrThrow(qr);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage($"*{qr}*");

        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<Ticket, bool>>>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void BuyCreateTicket_Creates_General_Ticket_And_Returns_Qr()
    {
        var userId = Guid.NewGuid();

        // Arrange
        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Ticket, bool>>>()))
            .Returns((Ticket?)null);

        _repo.Setup(r => r.Add(It.Is<Ticket>(t =>
                t.UserId == userId &&
                t.Type == TicketType.General &&
                t.SpecialEventId == null &&
                t.QrCode != Guid.Empty)))
            .Returns((Ticket t) => t);

        _user.Setup(u => u.GetByIdOrThrow(userId))
            .Returns(new User("Alice", "Baker", "a@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = userId });

        // Act
        var qr = _logic.BuyCreateTicket(userId, TicketType.General, null);

        // Assert
        qr.Should().NotBe(Guid.Empty);
        _user.Verify(u => u.GetByIdOrThrow(userId), Times.Once);
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<Ticket, bool>>>()), Times.AtLeastOnce);
        _repo.Verify(r => r.Add(It.Is<Ticket>(t =>
            t.UserId == userId &&
            t.Type == TicketType.General &&
            t.SpecialEventId == null)), Times.Once);
        _repo.VerifyNoOtherCalls();
        _events.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void BuyCreateTicket_Throws_When_SpecialEvent_Without_Id()
    {
        var userId = Guid.NewGuid();

        Action act = () => _logic.BuyCreateTicket(userId, TicketType.SpecialEvent, null);

        act.Should().Throw<ArgumentException>()
           .WithMessage("*SpecialEventId*");

        _repo.VerifyNoOtherCalls();
        _events.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void BuyCreateTicket_For_SpecialEvent_Validates_And_Adds()
    {
        var userId = Guid.NewGuid();
        var user = new User("Alice", "Baker", "a@x", "p", new DateOnly(1990,1,1), MembershipLevel.Standard)
            { Id = userId, Points = 10 };
        var evId = Guid.NewGuid();
        var ev = new SpecialEvent
        {
            Id = evId,
            Name = "Expo",
            StartDate = DateTime.UtcNow.AddHours(-1),
            EndDate = DateTime.UtcNow.AddHours(2),
            Capacity = 100
        };

        // Event exists
        _events.Setup(e => e.GetOrThrow(evId)).Returns(ev);

        // Count existing tickets for event (any expression ok)
        _repo.Setup(r => r.FindAll(It.IsAny<Expression<Func<Ticket, bool>>>()))
             .Returns(new List<Ticket> { new Ticket { Id = Guid.NewGuid(), QrCode = Guid.NewGuid(), Type = TicketType.SpecialEvent, UserId = Guid.NewGuid(), SpecialEventId = evId, Owner = user} });

        // EnsureSaleAllowed called with computed count
        _events.Setup(e => e.EnsureSaleAllowed(ev, 1, It.IsAny<DateTime>()));

        // QR unique checks → free
        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Ticket, bool>>>()))
             .Returns((Ticket?)null);

        // Add created ticket
        _repo.Setup(r => r.Add(It.Is<Ticket>(t =>
                t.UserId == userId &&
                t.Type == TicketType.SpecialEvent &&
                t.SpecialEventId == evId &&
                t.QrCode != Guid.Empty)))
             .Returns((Ticket t) => t);

        _user.Setup(u => u.GetByIdOrThrow(userId))
            .Returns(new User("Alice", "Baker", "a@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard)
            {
                Id = userId
            });
        var qr = _logic.BuyCreateTicket(userId, TicketType.SpecialEvent, evId);

        qr.Should().NotBe(Guid.Empty);

        _events.Verify(e => e.GetOrThrow(evId), Times.Once);
        _repo.Verify(r => r.FindAll(It.IsAny<Expression<Func<Ticket, bool>>>()), Times.Once);
        _events.Verify(e => e.EnsureSaleAllowed(ev, 1, It.IsAny<DateTime>()), Times.Once);
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<Ticket, bool>>>()), Times.AtLeastOnce);
        _repo.Verify(r => r.Add(It.Is<Ticket>(t => t.SpecialEventId == evId)), Times.Once);
        _repo.VerifyNoOtherCalls();
        _events.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void ValidateDateAndTimeTicketTest()
    {
        // Arrange
        var qrCode = Guid.NewGuid();
        var today = DateTime.UtcNow;
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = qrCode,
            VisitDate = DateOnly.FromDateTime(today),
            Type = TicketType.General,
            Owner = null,
            UserId = Guid.NewGuid()
        };

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Ticket, bool>>>()))
           .Returns(ticket);

        // Act
        var result = _logic.ValidateDateAndTimeTicket(qrCode, today);

        // Assert
        Assert.IsTrue(result);
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<Ticket, bool>>>()), Times.Once);
    }
}
