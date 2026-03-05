using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic.Test;

[TestClass]
public class AccessRecordLogicTest
{
    private Mock<IRepository<AccessRecord>> _accessRepo = null!;
    private Mock<ITicketLogic> _tickets = null!;
    private Mock<IAttractionHelperLogic> _attractions = null!;
    private Mock<IAwardPointsLogic> _awards = null!;
    private Mock<IUserLogic> _users = null!;
    private Mock<IIncidentLogic> _incident = null!;
    private Mock<ISpecialEventLogic> _specialEvents = null!;
    private Mock<IMaintenanceQueryLogic> _maintenance = null!; // <- NUEVO
    private AccessRecordLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _accessRepo = new Mock<IRepository<AccessRecord>>(MockBehavior.Strict);
        _tickets = new Mock<ITicketLogic>(MockBehavior.Strict);
        _attractions = new Mock<IAttractionHelperLogic>(MockBehavior.Strict);
        _awards = new Mock<IAwardPointsLogic>(MockBehavior.Strict);
        _users = new Mock<IUserLogic>(MockBehavior.Strict);
        _incident = new Mock<IIncidentLogic>(MockBehavior.Strict);
        _specialEvents = new Mock<ISpecialEventLogic>(MockBehavior.Strict);
        _maintenance = new Mock<IMaintenanceQueryLogic>(MockBehavior.Strict); // <- NUEVO

        // Por defecto, cuando un test llame Register(), NO hay mantenimiento
        _maintenance
            .Setup(m => m.IsAttractionUnderMaintenance(It.IsAny<Guid>(), It.IsAny<DateTime>()))
            .Returns(false);

        _logic = new AccessRecordLogic(
            _accessRepo.Object,
            _tickets.Object,
            _attractions.Object,
            _awards.Object,
            _users.Object,
            _incident.Object,
            _specialEvents.Object,
            _maintenance.Object   // <- NUEVO
        );
    }

    [TestMethod]
    public void Register_Throws_When_TicketOwnerMissing()
    {
        // arrange
        var now = DateTime.UtcNow;
        var ownerId = Guid.NewGuid();
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = ownerId,
            Owner = null,
            SpecialEventId = null,
            SpecialEvent = null,
            Type = TicketType.SpecialEvent
        };
        var attraction = new Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Zone",
            Type = AttractionType.InteractiveZone,
            MinAge = 0,
            MaxCapacity = 100,
            Description = "Fun",
            BasePoints = 3
        };

        var meta = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "MyStrategy",
            FileName = "strat.dll",
            FilePath = "/tmp/strat.dll",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        };

        _specialEvents
            .Setup(s => s.GetOrThrow(It.IsAny<Guid?>()))
            .Returns((Guid? id) => id.HasValue
                ? new SpecialEvent("tmp", DateTime.UtcNow, DateTime.UtcNow.AddDays(1), 1, 0m, new List<Attraction>()) { Id = id.Value }
                : null!);

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attraction.Id)).Returns(attraction);

        // act
        Action act = () => _logic.Register(ticket.QrCode, attraction.Id, now, meta.Id);

        // assert
        act.Should().Throw<KeyNotFoundException>()
           .WithMessage($"*{ownerId}*");

        _tickets.Verify(t => t.GetByQrOrThrow(ticket.QrCode), Times.Once);
        _attractions.Verify(a => a.GetOrThrow(attraction.Id), Times.Once);
        _awards.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _accessRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Register_Propagates_When_TicketNotFound()
    {
        // arrange
        var qr = Guid.NewGuid();
        var attractionId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        var scoringStrategyId = Guid.NewGuid();

        _tickets.Setup(t => t.GetByQrOrThrow(qr))
                .Throws(new KeyNotFoundException("Ticket with QR not found."));

        // act
        Action act = () => _logic.Register(qr, attractionId, now, scoringStrategyId);

        // assert
        act.Should().Throw<KeyNotFoundException>();

        _tickets.Verify(t => t.GetByQrOrThrow(qr), Times.Once);
        _attractions.VerifyNoOtherCalls();
        _awards.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _accessRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Register_Propagates_When_AttractionNotFound()
    {
        // arrange
        var now = DateTime.UtcNow;
        var owner = new User("Alice", "Baker", "a@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() };
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = owner.Id,
            Owner = owner,
            Type = TicketType.SpecialEvent
        };
        var attractionId = Guid.NewGuid();
        var meta = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "MyStrategy",
            FileName = "strat.dll",
            FilePath = "/tmp/strat.dll",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        };

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attractionId))
                    .Throws(new KeyNotFoundException("Attraction not found."));

        // act
        Action act = () => _logic.Register(ticket.QrCode, attractionId, now, meta.Id);

        // assert
        act.Should().Throw<KeyNotFoundException>();

        _tickets.Verify(t => t.GetByQrOrThrow(ticket.QrCode), Times.Once);
        _attractions.Verify(a => a.GetOrThrow(attractionId), Times.Once);
        _awards.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
        _accessRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void FindByAttractionAndDate_Filters_By_Attraction_And_Inclusive_Dates()
    {
        // arrange
        var targetAttrId = Guid.NewGuid();
        var otherAttrId = Guid.NewGuid();
        var start = new DateTime(2025, 1, 1);
        var end = new DateTime(2025, 1, 10);

        var ticket = new Ticket { Id = Guid.NewGuid(), QrCode = Guid.NewGuid(), Type = TicketType.General, Owner = new User("Alice", "Baker", "a@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard), UserId = Guid.NewGuid(), VisitDate = new DateOnly(2025, 1, 1) };
        var targetAttr = new Attraction { Id = targetAttrId, Name = "A", Description = "desc" };
        var otherAttr = new Attraction { Id = otherAttrId, Name = "B", Description = "desc" };

        var data = new List<AccessRecord>
        {
            new AccessRecord(new DateTime(2024, 12, 31), ticket, ticket.Id, targetAttr, targetAttrId, 0), // before
            new AccessRecord(new DateTime(2025, 01, 01), ticket, ticket.Id, targetAttr, targetAttrId, 0), // include (start)
            new AccessRecord(new DateTime(2025, 01, 05), ticket, ticket.Id, otherAttr,  otherAttrId,  0), // other attraction
            new AccessRecord(new DateTime(2025, 01, 10), ticket, ticket.Id, targetAttr, targetAttrId, 0), // include (end)
            new AccessRecord(new DateTime(2025, 01, 11), ticket, ticket.Id, targetAttr, targetAttrId, 0), // after
        };

        _accessRepo
            .Setup(r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<Func<AccessRecord, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<AccessRecord, object>>[]>()))
            .Returns((System.Linq.Expressions.Expression<Func<AccessRecord, bool>> pred,
                    System.Linq.Expressions.Expression<Func<AccessRecord, object>>[] _) =>
                data.Where(pred.Compile()).ToList());

        // act
        var result = _logic.FindByAttractionAndDate(targetAttrId, start, end);

        // assert
        result.Should().HaveCount(2);
        result.Select(r => r.InAt).Should().BeEquivalentTo(new[]
        {
            new DateTime(2025, 01, 01),
            new DateTime(2025, 01, 10)
        });

        _accessRepo.Verify(r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<Func<AccessRecord, bool>>>()), Times.Once);
        _accessRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void FindByAttractionAndDate_Returns_Empty_When_No_Match()
    {
        // arrange
        var targetAttrId = Guid.NewGuid();
        var start = new DateTime(2025, 2, 1);
        var end = new DateTime(2025, 2, 5);

        var ticket = new Ticket { Id = Guid.NewGuid(), QrCode = Guid.NewGuid(), Type = TicketType.General, Owner = new User("Alice", "Baker", "a@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard), UserId = Guid.NewGuid(), VisitDate = new DateOnly(2025, 1, 1) };
        var attr = new Attraction { Id = Guid.NewGuid(), Name = "A", Description = "desc" };

        var data = new List<AccessRecord>
        {
            new AccessRecord(new DateTime(2025, 1, 10), ticket, ticket.Id, attr, attr.Id, 0),
            new AccessRecord(new DateTime(2025, 3, 10), ticket, ticket.Id, attr, attr.Id, 0),
        };

        _accessRepo
            .Setup(r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<Func<AccessRecord, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<AccessRecord, object>>[]>()))
            .Returns((System.Linq.Expressions.Expression<Func<AccessRecord, bool>> pred,
                    System.Linq.Expressions.Expression<Func<AccessRecord, object>>[] _) =>
                data.Where(pred.Compile()).ToList());

        // act
        var result = _logic.FindByAttractionAndDate(targetAttrId, start, end);

        // assert
        result.Should().BeEmpty();
        _accessRepo.Verify(r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<Func<AccessRecord, bool>>>()), Times.Once);
        _accessRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void ValidateAgeTest()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var ticketQr = Guid.NewGuid();

        var attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test Ride",
            MinAge = 10,
            MaxCapacity = 50,
            Description = "A fun ride"
        };

        var user = new User("Test", "User", "t@u", "p", new DateOnly(2000, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() };
        var ticket = new Ticket
        {
            QrCode = ticketQr,
            Owner = user,
            UserId = user.Id,
            Type = TicketType.General,
            VisitDate = new DateOnly(2025, 1, 1)
        };

        _attractions.Setup(a => a.GetOrThrow(attractionId)).Returns(attraction);
        _tickets.Setup(t => t.GetByQrOrThrow(ticketQr)).Returns(ticket);
        _users.Setup(u => u.CalculateAge(user.Id)).Returns(25);

        // Act
        var result = _logic.ValidateAge(ticketQr, attractionId);

        // Assert
        Assert.IsTrue(result);
        _attractions.Verify(a => a.GetOrThrow(attractionId), Times.Once);
        _tickets.Verify(t => t.GetByQrOrThrow(ticketQr), Times.Once);
        _users.Verify(u => u.CalculateAge(user.Id), Times.Once);
    }

    [TestMethod]
    public void CheckCurrentCapacityTest()
    {
        // Arrange
        var attractionId = Guid.NewGuid();

        var attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test Ride",
            MinAge = 10,
            MaxCapacity = 50,
            Description = "A fun ride"
        };

        var ownerId = Guid.NewGuid();
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = ownerId,
            Owner = null,
            SpecialEventId = null,
            SpecialEvent = null,
            Type = TicketType.SpecialEvent
        };

        var accessRecord1 = new AccessRecord(
            inAt: new DateTime(2025, 4, 12),
            ticket: ticket,
            ticketId: ticket.Id,
            attraction: attraction,
            attractionId: attraction.Id,
            points: 10
        );

        var accessRecord2 = new AccessRecord(
            inAt: new DateTime(2025, 4, 12),
            ticket: ticket,
            ticketId: ticket.Id,
            attraction: attraction,
            attractionId: attraction.Id,
            points: 15
        );
        accessRecord2.OutAt = new DateTime(2025, 4, 13);

        var accessRecord3 = new AccessRecord(
            inAt: new DateTime(2025, 4, 12),
            ticket: ticket,
            ticketId: ticket.Id,
            attraction: attraction,
            attractionId: attraction.Id,
            points: 20
        );

        var data = new List<AccessRecord> { accessRecord1, accessRecord2, accessRecord3 };

        _accessRepo
            .Setup(r => r.FindAll(It.IsAny<System.Linq.Expressions.Expression<Func<AccessRecord, bool>>>(),
                                  It.IsAny<System.Linq.Expressions.Expression<Func<AccessRecord, object>>[]>()))
            .Returns((System.Linq.Expressions.Expression<Func<AccessRecord, bool>> pred,
                      System.Linq.Expressions.Expression<Func<AccessRecord, object>>[] _) =>
                data.Where(pred.Compile()).ToList());

        // Act
        var result = _logic.CheckCurrentCapacity(attractionId);

        // Assert
        Assert.AreEqual(result, 2);
    }

    [TestMethod]
    public void ValidateMaximumCapacityTest()
    {
        // Arrange
        var attractionId = Guid.NewGuid();

        var attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test Ride",
            MinAge = 10,
            MaxCapacity = 5,
            Description = "A fun ride"
        };

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Owner = null,
            Type = TicketType.SpecialEvent
        };

        var accessRecord1 = new AccessRecord(new DateTime(2025, 4, 12), ticket, ticket.Id, attraction, attractionId, 10);
        var accessRecord2 = new AccessRecord(new DateTime(2025, 4, 12), ticket, ticket.Id, attraction, attractionId, 15);
        var accessRecord3 = new AccessRecord(new DateTime(2025, 4, 12), ticket, ticket.Id, attraction, attractionId, 20);

        var data = new List<AccessRecord> { accessRecord1, accessRecord2, accessRecord3 };

        _accessRepo
            .Setup(r => r.FindAll(It.IsAny<Expression<Func<AccessRecord, bool>>>(),
                                  It.IsAny<Expression<Func<AccessRecord, object>>[]>()))
            .Returns((Expression<Func<AccessRecord, bool>> pred,
                      Expression<Func<AccessRecord, object>>[] _) => data.Where(pred.Compile()).ToList());

        _attractions.Setup(a => a.GetOrThrow(attractionId)).Returns(attraction);

        // Act
        var result = _logic.ValidateMaximumCapacity(attractionId);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void RegisterExitTest()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var accessRecordId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Owner = null,
            Type = TicketType.SpecialEvent
        };
        var attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test Ride",
            MaxCapacity = 5,
            Description = "Test"
        };

        var accessRecord = new AccessRecord(
            inAt: new DateTime(2025, 4, 12),
            ticket: ticket,
            ticketId: ticket.Id,
            attraction: attraction,
            attractionId: attractionId,
            points: 10
        )
        {
            Id = accessRecordId
        };

        _accessRepo.Setup(r => r.Find(It.IsAny<Expression<Func<AccessRecord, bool>>>()))
           .Returns<Expression<Func<AccessRecord, bool>>>(expr =>
               expr.Compile()(accessRecord) ? accessRecord : null);

        _accessRepo.Setup(r => r.Update(It.IsAny<AccessRecord>()))
        .Callback<AccessRecord>(ar => accessRecord = ar)
        .Returns<AccessRecord>(ar => ar);

        var nowUtc = new DateTime(2025, 4, 12);

        // Act
        _logic.RegisterExit(accessRecord.Id, nowUtc);

        // Assert
        Assert.IsNotNull(accessRecord.OutAt);
        Assert.AreEqual(nowUtc, accessRecord.OutAt);
        _accessRepo.Verify(r => r.Update(It.IsAny<AccessRecord>()), Times.Once);
    }

    [TestMethod]
    public void RemainingPeopleCapacityTest()
    {
        // Arrange
        var attractionId = Guid.NewGuid();

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Owner = null,
            Type = TicketType.SpecialEvent
        };

        var attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test Ride",
            MaxCapacity = 5,
            Description = "Fun ride"
        };

        var accessRecords = new List<AccessRecord>
        {
            new AccessRecord
            {
                InAt = DateTime.UtcNow,
                Ticket = ticket,
                TicketId = ticket.Id,
                Attraction = attraction,
                AttractionId = attraction.Id,
                OutAt = null,
                Points = 0
            },
            new AccessRecord
            {
                InAt = DateTime.UtcNow,
                Ticket = ticket,
                TicketId = ticket.Id,
                Attraction = attraction,
                AttractionId = attraction.Id,
                OutAt = null,
                Points = 0
            },
            new AccessRecord
            {
                InAt = DateTime.UtcNow,
                Ticket = ticket,
                TicketId = ticket.Id,
                Attraction = attraction,
                AttractionId = attraction.Id,
                OutAt = null,
                Points = 0
            }
        };

        _accessRepo
            .Setup(r => r.FindAll(It.IsAny<Expression<Func<AccessRecord, bool>>>()))
            .Returns(accessRecords);

        _attractions.Setup(a => a.GetOrThrow(attractionId)).Returns(attraction);

        // Act
        var remainingCapacity = _logic.RemainingPeopleCapacity(attractionId);

        // Assert
        Assert.AreEqual(2, remainingCapacity);
    }

    [TestMethod]
    public void ValidateSpecialEventTest()
    {
        // Arrange
        var ticketQr = Guid.NewGuid();
        var attractionId = Guid.NewGuid();
        var specialEventId = Guid.NewGuid();

        var attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test name",
            MinAge = 12,
            MaxCapacity = 100,
            Description = "Test",
            BasePoints = 10
        };

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = ticketQr,
            UserId = Guid.NewGuid(),
            Owner = null,
            Type = TicketType.SpecialEvent,
            SpecialEventId = specialEventId
        };

        var specialEvent = new SpecialEvent(
            "Evento Especial",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(1),
            100,
            50.0m,
            new List<Attraction> { attraction })
        {
            Id = specialEventId
        };

        _tickets.Setup(t => t.GetByQrOrThrow(ticketQr)).Returns(ticket);
        _specialEvents
            .Setup(s => s.IsAttractionReferenced(It.IsAny<Guid>()))
            .Returns(false);

        // Act
        var result = _logic.ValidateSpecialEvent(ticketQr, attractionId, new[] { specialEventId });

        // Assert
        Assert.IsFalse(result);
        _tickets.Verify(t => t.GetByQrOrThrow(ticketQr), Times.Once);
        _tickets.Verify(t => t.GetByQrOrThrow(ticketQr), Times.Once);
        _tickets.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Register_ReturnsPoints_AndPersistsAccess()
    {
        var ctx = ArrangeRegisterHappyPath(pointsToAward: 15);

        var result = _logic.Register(ctx.TicketQr, ctx.AttractionId, ctx.Now, ctx.StrategyId);

        result.Should().Be(15);
        ctx.Saved.Should().NotBeNull();
        ctx.Saved!.TicketId.Should().Be(ctx.TicketId);
        ctx.Saved.Ticket.Should().BeSameAs(ctx.Ticket);
        ctx.Saved.AttractionId.Should().Be(ctx.AttractionId);
        ctx.Saved.Attraction.Should().BeSameAs(ctx.Attraction);
        ctx.Saved.InAt.Should().Be(ctx.Now);
        ctx.Saved.OutAt.Should().BeNull();
        ctx.Saved.Points.Should().Be(15);
        ctx.Saved.SpecialEventId.Should().Be(ctx.Ticket.SpecialEventId);
    }

    [TestMethod]
    public void Register_Invokes_Collaborators_Once()
    {
        var ctx = ArrangeRegisterHappyPath(pointsToAward: 15);

        _ = _logic.Register(ctx.TicketQr, ctx.AttractionId, ctx.Now, ctx.StrategyId);

        _tickets.Verify(t => t.GetByQrOrThrow(ctx.TicketQr), Times.Once);
        _attractions.Verify(a => a.GetOrThrow(ctx.AttractionId), Times.AtLeastOnce());
        _awards.Verify(a => a.ComputeForAccess(ctx.Owner, It.IsAny<Domain.AccessRecord>(), ctx.Now, ctx.StrategyId), Times.Once);
        _users.Verify(u => u.AddPoints(ctx.Owner.Id, 15), Times.Once);
        _accessRepo.Verify(r => r.Add(It.IsAny<Domain.AccessRecord>()), Times.Once);
        _users.Verify(u => u.GetOrThrow(ctx.Owner.Id), Times.AtLeastOnce());
        _users.Verify(u => u.CalculateAge(ctx.Owner.Id), Times.AtLeastOnce());

        _accessRepo.Verify(r => r.FindAll(
                It.IsAny<Expression<Func<AccessRecord, bool>>>(),
                It.IsAny<Expression<Func<AccessRecord, object>>[]>()),
            Times.AtLeastOnce());

        _tickets.VerifyNoOtherCalls();
        _attractions.VerifyNoOtherCalls();
        _awards.VerifyNoOtherCalls();
        _users.VerifyNoOtherCalls();
    }

    private sealed class RegisterCtx
    {
        public DateTime Now { get; init; }
        public Guid TicketQr { get; init; }
        public Guid TicketId { get; init; }
        public Guid AttractionId { get; init; }
        public Domain.User Owner { get; init; } = default!;
        public Domain.Ticket Ticket { get; init; } = default!;
        public Domain.Attraction Attraction { get; init; } = default!;
        public Domain.AccessRecord? Saved { get; set; }
        public Guid StrategyId { get; init; }
    }

    private RegisterCtx ArrangeRegisterHappyPath(int pointsToAward)
    {
        var now = DateTime.UtcNow;

        var owner = new Domain.User("Alice", "Baker", "a@x", "p",
            new DateOnly(1990, 1, 1), Domain.MembershipLevel.Standard)
        { Id = Guid.NewGuid() };

        var ticket = new Domain.Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = owner.Id,
            Owner = owner,
            SpecialEventId = null,
            SpecialEvent = null,
            Type = Domain.TicketType.SpecialEvent
        };

        var attraction = new Domain.Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Coaster",
            Type = Domain.AttractionType.RollerCoaster,
            MinAge = 0,
            MaxCapacity = 10,
            Description = "Fast",
            BasePoints = 5
        };

        var meta = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "MyStrategy",
            FileName = "strat.dll",
            FilePath = "/tmp/strat.dll",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        };

        var ctx = new RegisterCtx
        {
            Now = now,
            TicketQr = ticket.QrCode,
            TicketId = ticket.Id,
            AttractionId = attraction.Id,
            Owner = owner,
            Ticket = ticket,
            Attraction = attraction,
            Saved = null,
            StrategyId = meta.Id,
        };

        // Mocks
        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attraction.Id)).Returns(attraction);

        _awards.Setup(a => a.ComputeForAccess(owner, It.IsAny<Domain.AccessRecord>(), now, meta.Id))
               .Returns(pointsToAward);

        _users.Setup(u => u.AddPoints(owner.Id, pointsToAward));
        _users.Setup(u => u.GetOrThrow(owner.Id)).Returns(owner);
        _users.Setup(u => u.CalculateAge(owner.Id)).Returns(30);

        _accessRepo
            .Setup(r => r.Add(It.IsAny<Domain.AccessRecord>()))
            .Callback<Domain.AccessRecord>(ar => ctx.Saved = ar)
            .Returns<Domain.AccessRecord>(ar => ar);

        _specialEvents
            .Setup(s => s.GetOrThrow(It.IsAny<Guid?>()))
            .Returns<Guid?>(id => new Domain.SpecialEvent(
                "dummy",
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow.AddDays(1),
                999,
                0m,
                new List<Domain.Attraction>())
            { Id = id ?? Guid.Empty });

        _specialEvents
            .Setup(s => s.IsAttractionReferenced(It.IsAny<Guid>()))
            .Returns(false);

        _accessRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<AccessRecord, bool>>>(),
                It.IsAny<Expression<Func<AccessRecord, object>>[]>()))
            .Returns((Expression<Func<AccessRecord, bool>> pred,
                      Expression<Func<AccessRecord, object>>[] _) =>
            {
                var open = new List<AccessRecord>();
                for(var i = 0; i < attraction.MaxCapacity - 1; i++)
                {
                    open.Add(new AccessRecord(now, ticket, ticket.Id, attraction, attraction.Id, 0));
                }

                return open.Where(pred.Compile()).ToList();
            });

        return ctx;
    }

    [TestMethod]
    public void Register_Throws_When_AgeRequirementNotMet()
    {
        // arrange
        var now = DateTime.UtcNow;

        var owner = new Domain.User("Kid", "User", "k@u", "p",
                new DateOnly(2015, 1, 1), Domain.MembershipLevel.Standard)
        { Id = Guid.NewGuid() };

        var ticket = new Domain.Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = owner.Id,
            Owner = owner,
            Type = Domain.TicketType.General,
            VisitDate = new DateOnly(2025, 1, 1)
        };

        var attraction = new Domain.Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Giant Coaster",
            Description = "Thrill",
            Type = Domain.AttractionType.RollerCoaster,
            MinAge = 18,
            MaxCapacity = 100,
            BasePoints = 5
        };

        var meta = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "MyStrategy",
            FileName = "strat.dll",
            FilePath = "/tmp/strat.dll",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        };

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attraction.Id)).Returns(attraction);

        _users.Setup(u => u.GetOrThrow(owner.Id)).Returns(owner);
        _users.Setup(u => u.CalculateAge(owner.Id)).Returns(10);

        _specialEvents
            .Setup(s => s.GetOrThrow(It.IsAny<Guid?>()))
            .Returns<Guid?>(_ => null!);

        _accessRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<AccessRecord, bool>>>(),
                It.IsAny<Expression<Func<AccessRecord, object>>[]>()))
            .Returns(new List<AccessRecord>());

        // act
        Action act = () => _logic.Register(ticket.QrCode, attraction.Id, now, meta.Id);

        // assert
        act.Should().Throw<Park.BusinessLogic.Exceptions.AgeRequirementNotMetException>();
    }

    [TestMethod]
    public void Register_Throws_When_AttractionDisabled()
    {
        // arrange
        var now = DateTime.UtcNow;
        var owner = new Domain.User("A", "B", "a@x", "p", new DateOnly(1990, 1, 1), Domain.MembershipLevel.Standard)
        { Id = Guid.NewGuid() };

        var ticket = new Domain.Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = owner.Id,
            Owner = owner,
            Type = Domain.TicketType.General,
            VisitDate = new DateOnly(2025, 1, 1)
        };

        var meta = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "MyStrategy",
            FileName = "strat.dll",
            FilePath = "/tmp/strat.dll",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        };

        var attractionId = Guid.NewGuid();

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);

        _attractions.Setup(a => a.GetOrThrow(attractionId))
            .Throws(new Park.BusinessLogic.Exceptions.AttractionDisabledException("Test Ride"));

        // act
        Action act = () => _logic.Register(ticket.QrCode, attractionId, now, meta.Id);

        // assert
        act.Should().Throw<Park.BusinessLogic.Exceptions.AttractionDisabledException>();
    }

    [TestMethod]
    public void Register_Throws_When_AttractionCapacityReached()
    {
        // arrange
        var now = DateTime.UtcNow;

        var meta = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "MyStrategy",
            FileName = "strat.dll",
            FilePath = "/tmp/strat.dll",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        };

        var owner = new Domain.User("A", "B", "a@x", "p",
            new DateOnly(1990, 1, 1), Domain.MembershipLevel.Standard)
        { Id = Guid.NewGuid() };

        var ticket = new Domain.Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = owner.Id,
            Owner = owner,
            Type = Domain.TicketType.General,
            VisitDate = new DateOnly(2025, 1, 1)
        };

        var attraction = new Domain.Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Full Room",
            Description = "desc",
            Type = Domain.AttractionType.InteractiveZone,
            MinAge = 0,
            MaxCapacity = 3,
            BasePoints = 1
        };

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attraction.Id)).Returns(attraction);

        _users.Setup(u => u.GetOrThrow(owner.Id)).Returns(owner);
        _users.Setup(u => u.CalculateAge(owner.Id)).Returns(30);

        var open = Enumerable.Range(0, attraction.MaxCapacity)
            .Select(_ => new Domain.AccessRecord(now, ticket, ticket.Id, attraction, attraction.Id, 0))
            .ToList();

        _accessRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<Domain.AccessRecord, bool>>>(),
                It.IsAny<Expression<Func<Domain.AccessRecord, object>>[]>()))
            .Returns((Expression<Func<Domain.AccessRecord, bool>> pred,
                      Expression<Func<Domain.AccessRecord, object>>[] _) =>
                     open.Where(pred.Compile()).ToList());

        _specialEvents.Setup(s => s.GetOrThrow(It.IsAny<Guid?>()))
                      .Returns<Guid?>(_ => null!);

        // act
        Action act = () => _logic.Register(ticket.QrCode, attraction.Id, now, meta.Id);

        // assert
        act.Should().Throw<Park.BusinessLogic.Exceptions.AttractionCapacityReachedException>();
    }

    [TestMethod]
    public void Register_Throws_When_SpecialEventAttractionMismatch()
    {
        // arrange
        var now = DateTime.UtcNow;

        var owner = new Domain.User("Alice", "Baker", "a@x", "p",
            new DateOnly(1990, 1, 1), Domain.MembershipLevel.Standard)
        { Id = Guid.NewGuid() };

        var attraction = new Domain.Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Haunted House",
            Description = "Spooky",
            Type = Domain.AttractionType.InteractiveZone,
            MinAge = 0,
            MaxCapacity = 100,
            BasePoints = 5
        };

        var meta = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "MyStrategy",
            FileName = "strat.dll",
            FilePath = "/tmp/strat.dll",
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.UtcNow
        };

        var ticketEventId = Guid.NewGuid();
        var ticketEvent = new Domain.SpecialEvent(
            "VIP Night", DateTime.UtcNow.AddHours(-1), DateTime.UtcNow.AddHours(2),
            500, 0m, new List<Domain.Attraction>())
        { Id = ticketEventId };

        var ticket = new Domain.Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = Guid.NewGuid(),
            UserId = owner.Id,
            Owner = owner,
            Type = Domain.TicketType.SpecialEvent,
            SpecialEventId = ticketEventId,
            SpecialEvent = ticketEvent
        };

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attraction.Id)).Returns(attraction);

        _users.Setup(u => u.GetOrThrow(owner.Id)).Returns(owner);
        _users.Setup(u => u.CalculateAge(owner.Id)).Returns(30);

        _specialEvents.Setup(s => s.IsAttractionReferenced(attraction.Id)).Returns(true);
        _specialEvents.Setup(s => s.GetEventIdsByAttraction(attraction.Id))
                      .Returns(new[] { Guid.NewGuid() });
        _specialEvents.Setup(s => s.GetOrThrow(It.IsAny<Guid?>()))
                      .Returns<Guid?>(id => id.HasValue ? ticketEvent : null!);

        _accessRepo.Setup(r => r.FindAll(
                It.IsAny<Expression<Func<Domain.AccessRecord, bool>>>(),
                It.IsAny<Expression<Func<Domain.AccessRecord, object>>[]>()))
            .Returns((Expression<Func<Domain.AccessRecord, bool>> pred,
                    Expression<Func<Domain.AccessRecord, object>>[] _) =>
                new List<Domain.AccessRecord>());

        // act
        Action act = () => _logic.Register(ticket.QrCode, attraction.Id, now, meta.Id);

        // assert
        act.Should()
           .Throw<Park.BusinessLogic.Exceptions.SpecialEventAttractionMismatchException>()
           .Where(e => e.Message.Contains("Haunted House") && e.Message.Contains("VIP Night"));
    }

    [TestMethod]
    public void Register_Throws_When_AttractionDisabled_Property()
    {
        var now = DateTime.UtcNow;

        var owner = new User("A", "B", "a@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() };
        var ticket = new Ticket { Id = Guid.NewGuid(), QrCode = Guid.NewGuid(), UserId = owner.Id, Owner = owner, Type = TicketType.General, VisitDate = new DateOnly(2025, 1, 1) };
        var attraction = new Attraction { Id = Guid.NewGuid(), Name = "attraction", Type = AttractionType.InteractiveZone, MinAge = 0, MaxCapacity = 2, Description = "desc", BasePoints = 1, Enabled = false };

        var strat = Guid.NewGuid();

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attraction.Id)).Returns(attraction);
        _users.Setup(u => u.GetOrThrow(owner.Id)).Returns(owner);
        _users.Setup(u => u.CalculateAge(owner.Id)).Returns(30);
        _accessRepo.Setup(r => r.FindAll(It.IsAny<Expression<Func<AccessRecord, bool>>>(),
                It.IsAny<Expression<Func<AccessRecord, object>>[]>()))
            .Returns(new List<AccessRecord>());

        _specialEvents.Setup(s => s.GetOrThrow(It.IsAny<Guid?>())).Returns<Guid?>(_ => null!);
        _specialEvents.Setup(s => s.IsAttractionReferenced(attraction.Id)).Returns(false);

        Action act = () => _logic.Register(ticket.QrCode, attraction.Id, now, strat);
        act.Should().Throw<AttractionDisabledException>()
            .Where(e => e.Message.Contains("attraction"));

        _awards.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Register_Throws_When_Age_Equals_MinAge()
    {
        var now = DateTime.UtcNow;

        var owner = new User("Teen", "U", "t@u", "p", new DateOnly(2007, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() };
        var ticket = new Ticket { Id = Guid.NewGuid(), QrCode = Guid.NewGuid(), UserId = owner.Id, Owner = owner, Type = TicketType.General, VisitDate = new DateOnly(2025, 1, 1) };
        var attraction = new Attraction { Id = Guid.NewGuid(), Name = "Edge", Type = AttractionType.InteractiveZone, MinAge = 18, MaxCapacity = 100, Description = "desc", BasePoints = 1 };

        var strat = Guid.NewGuid();

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attraction.Id)).Returns(attraction);
        _users.Setup(u => u.GetOrThrow(owner.Id)).Returns(owner);
        _users.Setup(u => u.CalculateAge(owner.Id)).Returns(18); // equal to MinAge
        _accessRepo.Setup(r => r.FindAll(It.IsAny<Expression<Func<AccessRecord, bool>>>(),
                It.IsAny<Expression<Func<AccessRecord, object>>[]>()))
            .Returns(new List<AccessRecord>());
        _specialEvents.Setup(s => s.GetOrThrow(It.IsAny<Guid?>())).Returns<Guid?>(_ => null!);
        _specialEvents.Setup(s => s.IsAttractionReferenced(attraction.Id)).Returns(false);

        Action act = () => _logic.Register(ticket.QrCode, attraction.Id, now, strat);
        act.Should().Throw<AgeRequirementNotMetException>();
    }

    [TestMethod]
    public void Register_Allows_When_SpecialEvent_Referenced_And_Matches()
    {
        var now = DateTime.UtcNow;
        var owner = new User("Alice", "B", "a@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() };
        var eventId = Guid.NewGuid();
        var ticket = new Ticket { Id = Guid.NewGuid(), QrCode = Guid.NewGuid(), UserId = owner.Id, Owner = owner, Type = TicketType.SpecialEvent, SpecialEventId = eventId };
        var attraction = new Attraction { Id = Guid.NewGuid(), Name = "Hall", Type = AttractionType.InteractiveZone, MinAge = 0, MaxCapacity = 5, Description = "desc", BasePoints = 1 };
        var strat = Guid.NewGuid();

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attraction.Id)).Returns(attraction);
        _users.Setup(u => u.GetOrThrow(owner.Id)).Returns(owner);
        _users.Setup(u => u.CalculateAge(owner.Id)).Returns(30);

        _accessRepo.Setup(r => r.FindAll(It.IsAny<Expression<Func<AccessRecord, bool>>>(),
                It.IsAny<Expression<Func<AccessRecord, object>>[]>()))
            .Returns(new List<AccessRecord>()); // not full

        _specialEvents.Setup(s => s.GetOrThrow(ticket.SpecialEventId)).Returns<Guid?>(id =>
            new SpecialEvent("VIP", now.AddHours(-1), now.AddHours(2), 100, 0m, new List<Attraction>()) { Id = id!.Value });

        _specialEvents.Setup(s => s.IsAttractionReferenced(attraction.Id)).Returns(true);
        _specialEvents.Setup(s => s.GetEventIdsByAttraction(attraction.Id)).Returns(new[] { eventId });

        _awards.Setup(a => a.ComputeForAccess(owner, It.IsAny<AccessRecord>(), now, strat)).Returns(7);
        _users.Setup(u => u.AddPoints(owner.Id, 7));
        _accessRepo.Setup(r => r.Add(It.IsAny<AccessRecord>())).Returns<AccessRecord>(ar => ar);

        var pts = _logic.Register(ticket.QrCode, attraction.Id, now, strat);

        pts.Should().Be(7);
    }

    [TestMethod]
    public void ValidateSpecialEvent_ReturnsTrue_When_Ticket_Not_SpecialEvent()
    {
        var qr = Guid.NewGuid();
        var ticket = new Ticket { Id = Guid.NewGuid(), QrCode = qr, Type = TicketType.General, Owner = new User("A", "B", "e", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() } };
        _tickets.Setup(t => t.GetByQrOrThrow(qr)).Returns(ticket);

        var ok = _logic.ValidateSpecialEvent(qr, Guid.NewGuid(), new[] { Guid.NewGuid() });
        Assert.IsTrue(ok);
    }

    [TestMethod]
    public void ValidateSpecialEvent_ReturnsTrue_When_SpecialEventId_Null()
    {
        var qr = Guid.NewGuid();
        var ticket = new Ticket { Id = Guid.NewGuid(), QrCode = qr, Type = TicketType.SpecialEvent, SpecialEventId = null, Owner = new User("A", "B", "e", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() } };
        _tickets.Setup(t => t.GetByQrOrThrow(qr)).Returns(ticket);

        var ok = _logic.ValidateSpecialEvent(qr, Guid.NewGuid(), new[] { Guid.NewGuid() });
        Assert.IsTrue(ok);
    }

    [TestMethod]
    public void ValidateSpecialEvent_ReturnsTrue_When_TicketEvent_Not_In_List()
    {
        var qr = Guid.NewGuid();
        var ev = Guid.NewGuid();
        var ticket = new Ticket { Id = Guid.NewGuid(), QrCode = qr, Type = TicketType.SpecialEvent, SpecialEventId = ev, Owner = new User("A", "B", "e", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() } };
        _tickets.Setup(t => t.GetByQrOrThrow(qr)).Returns(ticket);

        var ok = _logic.ValidateSpecialEvent(qr, Guid.NewGuid(), new[] { Guid.NewGuid(), Guid.NewGuid() });
        Assert.IsTrue(ok);
    }

    [TestMethod]
    public void ValidateAge_ReturnsFalse_When_Under_MinAge()
    {
        var attractionId = Guid.NewGuid();
        var qr = Guid.NewGuid();
        var attraction = new Attraction { Id = attractionId, Name = "X", MinAge = 18, MaxCapacity = 10, Description = "d" };
        var user = new User("T", "U", "t@u", "p", new DateOnly(2010, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() };
        var ticket = new Ticket { QrCode = qr, Owner = user, UserId = user.Id, Type = TicketType.General, VisitDate = new DateOnly(2025, 1, 1) };

        _attractions.Setup(a => a.GetOrThrow(attractionId)).Returns(attraction);
        _tickets.Setup(t => t.GetByQrOrThrow(qr)).Returns(ticket);
        _users.Setup(u => u.CalculateAge(user.Id)).Returns(17);

        Assert.IsFalse(_logic.ValidateAge(qr, attractionId));
    }

    [TestMethod]
    public void ValidateMaximumCapacity_ReturnsFalse_When_Over_Capacity()
    {
        var attractionId = Guid.NewGuid();
        var attraction = new Attraction { Id = attractionId, Name = "Room", Type = AttractionType.InteractiveZone, MinAge = 0, MaxCapacity = 2, Description = "desc", BasePoints = 1 };

        var t = new Ticket { Id = Guid.NewGuid(), QrCode = Guid.NewGuid(), Owner = new User("A", "B", "e", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() }, UserId = Guid.NewGuid(), Type = TicketType.General };
        var a1 = new AccessRecord(DateTime.UtcNow, t, t.Id, attraction, attractionId, 0);
        var a2 = new AccessRecord(DateTime.UtcNow, t, t.Id, attraction, attractionId, 0);
        var a3 = new AccessRecord(DateTime.UtcNow, t, t.Id, attraction, attractionId, 0);

        _accessRepo.Setup(r => r.FindAll(It.IsAny<Expression<Func<AccessRecord, bool>>>(),
                It.IsAny<Expression<Func<AccessRecord, object>>[]>()))
            .Returns(new List<AccessRecord> { a1, a2, a3 });

        _attractions.Setup(a => a.GetOrThrow(attractionId)).Returns(attraction);

        Assert.IsTrue(_logic.ValidateMaximumCapacity(attractionId));
    }

    [TestMethod]
    public void RegisterExit_Throws_When_Record_Not_Found()
    {
        var id = Guid.NewGuid();
        _accessRepo.Setup(r => r.Find(It.IsAny<Expression<Func<AccessRecord, bool>>>())).Returns((AccessRecord?)null);

        Action act = () => _logic.RegisterExit(id, DateTime.UtcNow);
        act.Should().Throw<KeyNotFoundException>()
            .Where(e => e.Message.Contains(id.ToString()));

        _accessRepo.Verify(r => r.Update(It.IsAny<AccessRecord>()), Times.Never);
    }

    [TestMethod]
    public void Register_Throws_When_Attraction_Under_Maintenance()
    {
        var now = DateTime.UtcNow;

        var owner = new User("U", "S", "u@s", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() };
        var ticket = new Ticket { Id = Guid.NewGuid(), QrCode = Guid.NewGuid(), UserId = owner.Id, Owner = owner, Type = TicketType.General, VisitDate = new DateOnly(2025, 1, 1) };
        var attraction = new Attraction { Id = Guid.NewGuid(), Name = "Ride", Type = AttractionType.InteractiveZone, MinAge = 0, MaxCapacity = 10, Description = "d", BasePoints = 1 };
        var strat = Guid.NewGuid();

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attraction.Id)).Returns(attraction);
        _users.Setup(u => u.GetOrThrow(owner.Id)).Returns(owner);
        _users.Setup(u => u.CalculateAge(owner.Id)).Returns(30);
        _specialEvents.Setup(s => s.GetOrThrow(It.IsAny<Guid?>())).Returns<Guid?>(_ => null!);

        _maintenance.Reset();
        _maintenance.Setup(m => m.IsAttractionUnderMaintenance(attraction.Id, now)).Returns(true);

        Action act = () => _logic.Register(ticket.QrCode, attraction.Id, now, strat);

        act.Should().Throw<InvalidOperationException>()
           .Where(e => e.Message.Contains("mantenimiento"));

        _accessRepo.Verify(r => r.Add(It.IsAny<AccessRecord>()), Times.Never);
    }

    [TestMethod]
    public void Register_Allows_When_Not_Under_Maintenance()
    {
        var now = DateTime.UtcNow;

        var owner = new User("U", "S", "u@s", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = Guid.NewGuid() };
        var ticket = new Ticket { Id = Guid.NewGuid(), QrCode = Guid.NewGuid(), UserId = owner.Id, Owner = owner, Type = TicketType.General, VisitDate = new DateOnly(2025, 1, 1) };
        var attraction = new Attraction { Id = Guid.NewGuid(), Name = "Ride", Type = AttractionType.InteractiveZone, MinAge = 0, MaxCapacity = 10, Description = "d", BasePoints = 1 };
        var strat = Guid.NewGuid();

        _tickets.Setup(t => t.GetByQrOrThrow(ticket.QrCode)).Returns(ticket);
        _attractions.Setup(a => a.GetOrThrow(attraction.Id)).Returns(attraction);
        _users.Setup(u => u.GetOrThrow(owner.Id)).Returns(owner);
        _users.Setup(u => u.CalculateAge(owner.Id)).Returns(30);

        _specialEvents.Setup(s => s.IsAttractionReferenced(attraction.Id)).Returns(false);
        _specialEvents.Setup(s => s.GetOrThrow(It.Is<Guid?>(g => !g.HasValue)))
            .Returns((SpecialEvent?)null);
        _specialEvents.Setup(s => s.GetEventIdsByAttraction(It.IsAny<Guid>()))
            .Returns(Array.Empty<Guid>());

        _maintenance.Reset();
        _maintenance.Setup(m => m.IsAttractionUnderMaintenance(attraction.Id, now)).Returns(false);

        _awards.Setup(a => a.ComputeForAccess(owner, It.IsAny<AccessRecord>(), now, strat)).Returns(5);
        _users.Setup(u => u.AddPoints(owner.Id, 5));
        _accessRepo.Setup(r => r.FindAll(
                It.IsAny<Expression<Func<AccessRecord, bool>>>(),
                It.IsAny<Expression<Func<AccessRecord, object>>[]>()))
            .Returns(new List<AccessRecord>());
        _accessRepo.Setup(r => r.Add(It.IsAny<AccessRecord>()))
            .Returns<AccessRecord>(ar => ar);

        var pts = _logic.Register(ticket.QrCode, attraction.Id, now, strat);

        pts.Should().Be(5);
        _accessRepo.Verify(r => r.Add(It.IsAny<AccessRecord>()), Times.Once);
    }
}
