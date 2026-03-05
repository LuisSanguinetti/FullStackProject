using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class SpecialEventLogicTest
{
    private Mock<IRepository<SpecialEvent>> _repo = null!;
    private SpecialEventLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new Mock<IRepository<SpecialEvent>>(MockBehavior.Strict);
        _logic = new SpecialEventLogic(_repo.Object);
    }

    [TestMethod]
    public void GetOrThrow_Returns_Event_When_Found()
    {
        var id = Guid.NewGuid();
        var ev = new SpecialEvent
        {
            Id = id,
            Name = "Concert",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            Capacity = 100
        };

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<SpecialEvent, bool>>>()))
             .Returns((Expression<Func<SpecialEvent, bool>> pred) =>
                 pred.Compile()(ev) ? ev : null);

        var result = _logic.GetOrThrow(id);

        result.Should().BeSameAs(ev);
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<SpecialEvent, bool>>>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetOrThrow_Throws_When_NotFound()
    {
        var id = Guid.NewGuid();
        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<SpecialEvent, bool>>>()))
             .Returns((SpecialEvent?)null);

        Action act = () => _logic.GetOrThrow(id);

        act.Should().Throw<KeyNotFoundException>()
           .WithMessage($"*{id}*");

        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<SpecialEvent, bool>>>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void EnsureSaleAllowed_Allows_When_Within_Window_And_Capacity()
    {
        var now = DateTime.UtcNow;
        var ev = new SpecialEvent
        {
            Id = Guid.NewGuid(),
            Name = "Expo",
            StartDate = now.AddHours(-1),
            EndDate = now.AddHours(1),
            Capacity = 50
        };

        Action act = () => _logic.EnsureSaleAllowed(ev, ticketsSold: 10, nowUtc: now);

        act.Should().NotThrow();
    }

    [TestMethod]
    public void EnsureSaleAllowed_Throws_When_Outside_Window_Early()
    {
        var now = DateTime.UtcNow;
        var ev = new SpecialEvent
        {
            Id = Guid.NewGuid(),
            Name = "Expo",
            StartDate = now.AddHours(2),
            EndDate = now.AddHours(4),
            Capacity = 50
        };

        Action act = () => _logic.EnsureSaleAllowed(ev, ticketsSold: 0, nowUtc: now);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*event window*");
    }

    [TestMethod]
    public void EnsureSaleAllowed_Throws_When_Capacity_Reached()
    {
        var now = DateTime.UtcNow;
        var ev = new SpecialEvent
        {
            Id = Guid.NewGuid(),
            Name = "Expo",
            StartDate = now.AddHours(-2),
            EndDate = now.AddHours(2),
            Capacity = 100
        };

        Action act = () => _logic.EnsureSaleAllowed(ev, ticketsSold: 100, nowUtc: now);

        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*capacity*");
    }

    [TestMethod]
    public void EnsureSaleAllowed_Throws_When_Outside_Window_Late()
    {
        var now = DateTime.UtcNow;
        var ev = new SpecialEvent
        {
            Id = Guid.NewGuid(),
            Name = "Expo",
            StartDate = now.AddHours(-4),
            EndDate = now.AddHours(-1),
            Capacity = 50
        };

        Action act = () => _logic.EnsureSaleAllowed(ev, ticketsSold: 0, nowUtc: now);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*event window*");
    }

    [TestMethod]
    public void IsAttractionReferenced_ReturnsTrue_When_EventContainsAttraction()
    {
        // arrange
        var attractionId = Guid.NewGuid();
        var ev = new SpecialEvent
        {
            Id = Guid.NewGuid(),
            Name = "WithRef",
            StartDate = DateTime.UtcNow.AddHours(-1),
            EndDate = DateTime.UtcNow.AddHours(1),
            Capacity = 10,
            Attractions = new List<Attraction>
            {
                new Attraction { Id = attractionId, Name = "A", Type = AttractionType.RollerCoaster, MinAge = 0, MaxCapacity = 1, Description = "d" }
            }
        };

        _repo.Setup(r => r.FindAll(
                It.IsAny<Expression<Func<SpecialEvent, bool>>>(),
                It.IsAny<Expression<Func<SpecialEvent, object>>[]>()))
            .Returns(new List<SpecialEvent> { ev });

        // act
        var result = _logic.IsAttractionReferenced(attractionId);

        // assert
        Assert.IsTrue(result);
        _repo.Verify(r => r.FindAll(
            It.IsAny<Expression<Func<SpecialEvent, bool>>>(),
            It.IsAny<Expression<Func<SpecialEvent, object>>[]>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void IsAttractionReferenced_ReturnsFalse_When_NoEventContainsAttraction()
    {
        // arrange
        var attractionId = Guid.NewGuid();
        var ev = new SpecialEvent
        {
            Id = Guid.NewGuid(),
            Name = "NoRef",
            StartDate = DateTime.UtcNow.AddHours(-1),
            EndDate = DateTime.UtcNow.AddHours(1),
            Capacity = 10,
            Attractions = new List<Attraction>
            {
                new Attraction { Id = Guid.NewGuid(), Name = "B", Type = AttractionType.Simulator, MinAge = 0, MaxCapacity = 1, Description = "x" }
            }
        };

        _repo.Setup(r => r.FindAll(
                It.IsAny<Expression<Func<SpecialEvent, bool>>>(),
                It.IsAny<Expression<Func<SpecialEvent, object>>[]>()))
            .Returns(() => (IList<SpecialEvent>?)null);

        // act
        var result = _logic.IsAttractionReferenced(attractionId);

        // assert
        Assert.IsFalse(result);
        _repo.Verify(r => r.FindAll(
            It.IsAny<Expression<Func<SpecialEvent, bool>>>(),
            It.IsAny<Expression<Func<SpecialEvent, object>>[]>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void IsAttractionReferenced_ReturnsFalse_When_RepoReturnsNull()
    {
        // arrange
        var attractionId = Guid.NewGuid();

        _repo.Setup(r => r.FindAll(It.IsAny<Expression<Func<SpecialEvent, bool>>>(), It.IsAny<Expression<Func<SpecialEvent, object>>[]>()))
            .Returns(() => (IList<SpecialEvent>?)null);

        // act
        var result = _logic.IsAttractionReferenced(attractionId);

        // assert
        Assert.IsFalse(result);
        _repo.Verify(r => r.FindAll(
            It.IsAny<Expression<Func<SpecialEvent, bool>>>(),
            It.IsAny<Expression<Func<SpecialEvent, object>>[]>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }
}

[TestClass]
public class SpecialEventAdminLogicTest
{
    private Mock<IRepository<SpecialEvent>> _eventRepo = null!;
    private Mock<IRepository<Attraction>> _attrRepo = null!;
    private Mock<IRepository<Ticket>> _ticketRepo = null!;
    private Mock<ISystemClock> _clock = null!;
    private ISpecialEventAdminLogic _logic = null!;
    private ISpecialEventLogic _specialEventLogic = null!;

    [TestInitialize]
    public void Setup()
    {
        _eventRepo = new Mock<IRepository<SpecialEvent>>();
        _attrRepo = new Mock<IRepository<Attraction>>();
        _ticketRepo = new Mock<IRepository<Ticket>>();
        _clock = new Mock<ISystemClock>();

        _clock.Setup(c => c.Now()).Returns(new DateTime(2025, 1, 1, 10, 0, 0, DateTimeKind.Utc)); // �hora actual� controlada

        _logic = new Park.BusinessLogic.SpecialEventAdminLogic(
            _eventRepo.Object, _attrRepo.Object, _ticketRepo.Object, _clock.Object);
        _specialEventLogic = new Park.BusinessLogic.SpecialEventLogic(_eventRepo.Object);
    }

    [TestMethod]
    public void Create_Valid_SetsId_And_Persists()
    {
        var atts = new[] { Guid.NewGuid(), Guid.NewGuid() };

        _attrRepo.Setup(r => r.FindAll(It.IsAny<Expression<Func<Attraction, bool>>>(), Array.Empty<Expression<Func<Attraction, object>>>()))
                 .Returns(new List<Attraction>
                {
                     new Attraction("A", AttractionType.RollerCoaster, 10, 10, "d", 0){ Enabled=true, Id=atts[0] },
                     new Attraction("B", AttractionType.Show, 0, 100, "d", 0){ Enabled=true, Id=atts[1] }
                 });

        _eventRepo.Setup(r => r.Add(It.IsAny<SpecialEvent>())).Returns<SpecialEvent>(e => e);

        var ev = _logic.Create(
            name: "Noche tem�tica",
            start: new DateTime(2025, 1, 2, 20, 0, 0, DateTimeKind.Utc),
            end: new DateTime(2025, 1, 2, 23, 0, 0, DateTimeKind.Utc),
            capacity: 200,
            extraPrice: 150,
            attractionIds: atts
        );

        ev.Id.Should().NotBe(Guid.Empty);
        ev.Capacity.Should().Be(200);
        ev.ExtraPrice.Should().Be(150);
        ev.Attractions.Should().HaveCount(2);
    }

    [TestMethod]
    public void Create_Fails_When_AnyAttractionMissingOrDisabled()
    {
        var atts = new[] { Guid.NewGuid(), Guid.NewGuid() };

        // Solo devolver UNA atracci�n (falta la otra) o deshabilitada
        _attrRepo.Setup(r => r.FindAll(It.IsAny<Expression<Func<Attraction, bool>>>(), Array.Empty<Expression<Func<Attraction, object>>>()))
                 .Returns(new List<Attraction>
                {
                     new Attraction("A", AttractionType.Show, 0, 100, "d", 0){ Enabled=false, Id=atts[0] }
                 });

        Action act = () => _logic.Create("Noche", new DateTime(2025, 1, 2, 20, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 2, 23, 0, 0, DateTimeKind.Utc),
                                         50, 100, atts);
        act.Should().Throw<ArgumentException>().WithMessage("*attractions*");
    }

    [TestMethod]
    public void Create_Fails_When_DateInPast()
    {
        _attrRepo.Setup(r => r.FindAll(It.IsAny<Expression<Func<Attraction, bool>>>(), Array.Empty<Expression<Func<Attraction, object>>>()))
                 .Returns(new List<Attraction>());

        Action act = () => _logic.Create("Noche", new DateTime(2024, 12, 31, 20, 0, 0, DateTimeKind.Utc), new DateTime(2024, 12, 31, 22, 0, 0, DateTimeKind.Utc),
                                         50, 100, Array.Empty<Guid>());
        act.Should().Throw<ArgumentException>().WithMessage("*past*");
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Create_Fails_When_Capacity_Not_Positive(int cap)
    {
        _attrRepo.Setup(r => r.FindAll(It.IsAny<Expression<Func<Attraction, bool>>>(), Array.Empty<Expression<Func<Attraction, object>>>()))
                 .Returns(new List<Attraction>());

        Action act = () => _logic.Create("Noche", new DateTime(2025, 1, 2, 20, 0, 0, DateTimeKind.Utc), new DateTime(2025, 1, 2, 22, 0, 0, DateTimeKind.Utc),
                                         cap, 100, Array.Empty<Guid>());
        act.Should().Throw<ArgumentException>().WithMessage("*capacity*");
    }

    [TestMethod]
    public void Delete_Blocked_When_Tickets_Sold()
    {
        var evId = Guid.NewGuid();

        _eventRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<SpecialEvent, bool>>>()))
            .Returns(new SpecialEvent
            {
                Id = evId,
                Name = "Event X",
                StartDate = new DateTime(2025, 1, 2, 20, 0, 0, DateTimeKind.Utc),
                EndDate   = new DateTime(2025, 1, 2, 23, 0, 0, DateTimeKind.Utc),
                Capacity = 100,
                ExtraPrice = 0m
            });

        var u = new User("John","Doe","john@x","p", new DateOnly(2000,1,1), MembershipLevel.Standard);

        _ticketRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                Array.Empty<Expression<Func<Ticket, object>>>()))
            .Returns(new List<Ticket>
            {
                new Ticket(
                    Guid.NewGuid(),
                    new DateOnly(2025, 1, 2),
                    TicketType.SpecialEvent,
                    u,
                    u.Id,
                    specialEvent: null!, // not needed by test
                    specialEventId: evId)
            });

        Action act = () => _logic.Delete(evId);
        act.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void Delete_Allows_When_No_Tickets()
    {
        var evId = Guid.NewGuid();

        _eventRepo
            .Setup(r => r.Find(It.IsAny<Expression<Func<SpecialEvent, bool>>>()))
            .Returns(new SpecialEvent
            {
                Id = evId,
                Name = "Event Y",
                StartDate = new DateTime(2025, 1, 3, 20, 0, 0, DateTimeKind.Utc),
                EndDate   = new DateTime(2025, 1, 3, 23, 0, 0, DateTimeKind.Utc),
                Capacity = 50,
                ExtraPrice = 0m
            });

        _ticketRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<Ticket, bool>>>(),
                Array.Empty<Expression<Func<Ticket, object>>>()))
            .Returns(new List<Ticket>());

        _logic.Delete(evId);

        _eventRepo.Verify(r => r.Delete(evId), Times.Once);
    }

    [TestMethod]
    public void GetEventIdsByAttraction_Filters_By_Attraction_And_Returns_Ids()
    {
        // arrange
        var aMatch = Guid.NewGuid();
        var aOther = Guid.NewGuid();

        var attMatch = new Attraction { Id = aMatch, Name = "Match", Description = " new description"};
        var attOther = new Attraction { Id = aOther, Name = "Other", Description = "new description 2"};

        var ev1 = new SpecialEvent("E1", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(2), 100, 0m, new List<Attraction> { attMatch })
            { Id = Guid.NewGuid() };
        var ev2 = new SpecialEvent("E2", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(2), 100, 0m, new List<Attraction> { attOther })
            { Id = Guid.NewGuid() };
        var ev3 = new SpecialEvent("E3", DateTime.UtcNow.AddDays(-2), DateTime.UtcNow.AddDays(2), 100, 0m, new List<Attraction> { attMatch, attOther })
            { Id = Guid.NewGuid() };

        var data = new List<SpecialEvent> { ev1, ev2, ev3 };

        _eventRepo
            .Setup(r => r.FindAll(
                It.IsAny<Expression<Func<SpecialEvent, bool>>>(),
                It.IsAny<Expression<Func<SpecialEvent, object>>[]>()))
            .Returns((Expression<Func<SpecialEvent, bool>> pred,
                    Expression<Func<SpecialEvent, object>>[] _) =>
                data.Where(pred.Compile()).ToList());

        // act
        var ids = _specialEventLogic.GetEventIdsByAttraction(aMatch).ToList();

        // assert
        ids.Should().BeEquivalentTo(new[] { ev1.Id, ev3.Id });
        _eventRepo.Verify(r => r.FindAll(
            It.IsAny<Expression<Func<SpecialEvent, bool>>>(),
            It.IsAny<Expression<Func<SpecialEvent, object>>[]>()), Times.Once);
        _eventRepo.VerifyNoOtherCalls();
        _attrRepo.VerifyNoOtherCalls();
    }
}
