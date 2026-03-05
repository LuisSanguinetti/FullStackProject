using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class AttractionLogicTest
{
    private Mock<IRepository<Attraction>> _attrRepoMock = null!;
    private Mock<IAccessRecordLogic> _accessLogicMock = null!;
    private AttractionLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _attrRepoMock = new Mock<IRepository<Attraction>>(MockBehavior.Strict);
        _accessLogicMock = new Mock<IAccessRecordLogic>(MockBehavior.Strict);

        _attrRepoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()))
            .Returns((Attraction?)null);

        _logic = new AttractionLogic(_accessLogicMock.Object, _attrRepoMock.Object);
    }

    [TestMethod]
    public void NumberOfVisitsTest()
    {
        // arrange
        var attractionId = Guid.NewGuid();
        var attraction = new Attraction
        {
            Id = attractionId,
            Name = "TestAttraction",
            Type = 0,
            MinAge = 6,
            MaxCapacity = 100,
            Description = "Test attraction"
        };

        var ticket = new Ticket
        {
            QrCode = Guid.NewGuid(),
            Type = 0,
            Owner = new User("Alice", "Baker", "alice@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard),
            UserId = Guid.NewGuid(),
            VisitDate = new DateOnly(2025, 1, 1)
        };

        var startDate = new DateTime(2025, 1, 1);
        var endDate = new DateTime(2025, 1, 10);

        var records = new List<AccessRecord>
        {
            new AccessRecord(new DateTime(2025, 1, 8), ticket, Guid.NewGuid(), attraction, attractionId, 0),
            new AccessRecord(new DateTime(2025, 1, 3), ticket, Guid.NewGuid(), attraction, attractionId, 0)
        };

        _attrRepoMock
            .Setup(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()))
            .Returns(attraction);

        _accessLogicMock
            .Setup(l => l.FindByAttractionAndDate(attractionId, startDate, endDate))
            .Returns(records);

        // act
        var result = _logic.NumberOfVisits(attractionId, startDate, endDate);

        // assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result);
    }

    [TestMethod]
    public void GetOrThrow_Returns_Attraction_When_Found()
    {
        var id = Guid.NewGuid();
        var attr = new Attraction
        {
            Id = id,
            Name = "Coaster",
            Type = AttractionType.RollerCoaster,
            MinAge = 0,
            MaxCapacity = 20,
            Description = "Fast",
            BasePoints = 7
        };

        _attrRepoMock.Setup(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()))
            .Returns((Expression<Func<Attraction, bool>> pred) =>
                pred.Compile()(attr) ? attr : null);

        var result = _logic.GetOrThrow(id);

        result.Should().BeSameAs(attr);
        _attrRepoMock.Verify(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()), Times.Once);
        _attrRepoMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetOrThrow_Throws_When_NotFound()
    {
        var id = Guid.NewGuid();

        _attrRepoMock.Setup(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()))
            .Returns((Attraction?)null);

        Action act = () => _logic.GetOrThrow(id);

        act.Should().Throw<KeyNotFoundException>()
            .WithMessage($"*{id}*");

        _attrRepoMock.Verify(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()), Times.Once);
        _attrRepoMock.VerifyNoOtherCalls();
    }
}

[TestClass]
public class AttractionAdminLogicTest_InSameFile
{
    private Moq.Mock<IDataAccess.IRepository<Domain.Attraction>> _attrRepo = null!;
    private Park.BusinessLogic.AttractionAdminLogic _logic = null!;
    private Moq.Mock<IParkBusinessLogic.IIncidentLogic> _incidentLogic = null!;
    private Moq.Mock<IParkBusinessLogic.ISpecialEventLogic> _eventLogic = null!;

    [TestInitialize]
    public void SetupAdmin()
    {
        _attrRepo = new Moq.Mock<IDataAccess.IRepository<Domain.Attraction>>();
        _incidentLogic = new Moq.Mock<IParkBusinessLogic.IIncidentLogic>();
        _eventLogic = new Moq.Mock<IParkBusinessLogic.ISpecialEventLogic>();
        _logic = new Park.BusinessLogic.AttractionAdminLogic(_attrRepo.Object, _incidentLogic.Object, _eventLogic.Object);
    }

    [TestMethod]
    public void Create_Valid_SetsId_And_Enabled()
    {
        _attrRepo.Setup(r => r.Add(Moq.It.IsAny<Domain.Attraction>())).Returns<Domain.Attraction>(a => a);
        var a = _logic.Create("Coaster", Domain.AttractionType.RollerCoaster, 12, 24, "desc", 10);
        Assert.AreNotEqual(Guid.Empty, a.Id);
        Assert.IsTrue(a.Enabled);
    }

    [DataTestMethod]
    [DataRow(-1, 10)]
    [DataRow(10, 0)]
    [DataRow(10, -5)]
    public void Create_Invalid_Throws(int minAge, int capacity)
    {
        Assert.ThrowsException<ArgumentException>(() =>
            _logic.Create("X", Domain.AttractionType.Simulator, minAge, capacity, "d", 0));
    }

    [TestMethod]
    public void Delete_Blocked_WhenActiveIncidents()
    {
        var id = Guid.NewGuid();

        var a = new Domain.Attraction
        {
            Id = id, Name = "A", Type = Domain.AttractionType.Simulator,
            MinAge = 0, MaxCapacity = 1, Description = "d", BasePoints = 0, Enabled = true
        };
        _attrRepo.Setup(r => r.Find(Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>>>()))
            .Returns(a);

        _incidentLogic
               .Setup(x => x.HasActiveIncidents(id))
               .Returns(true);

        Assert.ThrowsException<InvalidOperationException>(() => _logic.Delete(id));
    }

    [TestMethod]
    public void Delete_Blocked_WhenReferencedByEvent()
    {
        var id = Guid.NewGuid();
        var a = new Domain.Attraction { Id = id, Name = "A", Type = Domain.AttractionType.Show, MinAge = 0, MaxCapacity = 10, Description = "d", BasePoints = 0, Enabled = true };
        _attrRepo.Setup(r => r.Find(Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>>>())).Returns(a);
        _eventLogic
            .Setup(x => x.IsAttractionReferenced(id))
            .Returns(true);

        Assert.ThrowsException<InvalidOperationException>(() => _logic.Delete(id));
    }

    [TestMethod]
    public void List_Filter_ByType_And_Enabled()
    {
        var list = new List<Domain.Attraction>
        {
                new Domain.Attraction("A", Domain.AttractionType.RollerCoaster,12,10,"d",0){ Enabled=true },
                new Domain.Attraction("B", Domain.AttractionType.Simulator,10,10,"d",0){ Enabled=false }
            };

        _attrRepo.Setup(r => r.FindAll(
                Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>>>(),
                Array.Empty<System.Linq.Expressions.Expression<Func<Domain.Attraction, object>>>()))
                 .Returns(list.Where(a => a.Enabled && a.Type == Domain.AttractionType.RollerCoaster).ToList());

        var res = _logic.List(Domain.AttractionType.RollerCoaster, true).ToList();
        Assert.AreEqual(1, res.Count);
        Assert.AreEqual("A", res[0].Name);
    }

    [TestMethod]
    public void Admin_Create_Invalid_Throws()
    {
        Assert.ThrowsException<ArgumentException>(() => _logic.Create("X", Domain.AttractionType.Simulator, -1, 10, "d"));
        Assert.ThrowsException<ArgumentException>(() => _logic.Create("X", Domain.AttractionType.Simulator, 0, 0, "d"));
    }

    [TestMethod]
    public void SetEnabled_SetsFlag_And_Persists()
    {
        // arrange
        var id = Guid.NewGuid();
        var a = new Domain.Attraction
        {
            Id = id, Name = "A", Type = Domain.AttractionType.Show,
            MinAge = 0, MaxCapacity = 10, Description = "d", BasePoints = 0, Enabled = false
        };

        _attrRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()))
                 .Returns(a);
        _attrRepo.Setup(r => r.Update(It.IsAny<Domain.Attraction>()))
                 .Returns<Domain.Attraction>(x => x);

        // act
        _logic.SetEnabled(id, true);

        // assert
        _attrRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()), Times.Once);
        _attrRepo.Verify(r => r.Update(It.Is<Domain.Attraction>(x => x.Id == id && x.Enabled == true)), Times.Once);
        _attrRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void SetEnabled_Throws_When_NotFound()
    {
        // arrange
        var id = Guid.NewGuid();
        _attrRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()))
                 .Returns((Domain.Attraction?)null);

        // act
        Action act = () => _logic.SetEnabled(id, true);

        // assert
        Assert.ThrowsException<KeyNotFoundException>(() => act());
        _attrRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()), Times.Once);
        _attrRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetOrThrow_Returns_Attraction()
    {
        // arrange
        var id = Guid.NewGuid();
        var a = new Domain.Attraction
        {
            Id = id, Name = "A", Type = Domain.AttractionType.Simulator,
            MinAge = 0, MaxCapacity = 5, Description = "d", BasePoints = 0, Enabled = true
        };

        _attrRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()))
                 .Returns((Expression<Func<Domain.Attraction, bool>> pred) =>
                     pred.Compile()(a) ? a : null);

        // act
        var result = _logic.GetOrThrow(id);

        // assert
        result.Should().BeSameAs(a);
        _attrRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()), Times.Once);
        _attrRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetOrThrow_Throws_When_NotFound()
    {
        // arrange
        var id = Guid.NewGuid();
        _attrRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()))
                 .Returns((Domain.Attraction?)null);

        // act
        Action act = () => _logic.GetOrThrow(id);

        // assert
        act.Should().Throw<KeyNotFoundException>().WithMessage($"*{id}*");
        _attrRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()), Times.Once);
        _attrRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Update_Updates_All_Fields_And_Persists()
    {
        // arrange
        var id = Guid.NewGuid();
        var a = new Domain.Attraction
        {
            Id = id, Name = "Old", Type = Domain.AttractionType.Show,
            MinAge = 1, MaxCapacity = 5, Description = "old", BasePoints = 1, Enabled = true
        };

        _attrRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()))
                 .Returns(a);
        _attrRepo.Setup(r => r.Update(It.IsAny<Domain.Attraction>()))
                 .Returns<Domain.Attraction>(x => x);

        // act
        var updated = _logic.Update(
            id,
            name: "New",
            type: Domain.AttractionType.Simulator,
            minAge: 3,
            capacity: 10,
            description: "new",
            basePoints: 7,
            enabled: null // don't change flag here
        );

        // assert
        Assert.AreEqual("New", updated.Name);
        Assert.AreEqual(Domain.AttractionType.Simulator, updated.Type);
        Assert.AreEqual(3, updated.MinAge);
        Assert.AreEqual(10, updated.MaxCapacity);
        Assert.AreEqual("new", updated.Description);
        Assert.AreEqual(7, updated.BasePoints);
        Assert.IsTrue(updated.Enabled); // unchanged

        _attrRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()), Times.Once);
        _attrRepo.Verify(r => r.Update(It.Is<Domain.Attraction>(x =>
            x.Id == id &&
            x.Name == "New" &&
            x.Type == Domain.AttractionType.Simulator &&
            x.MinAge == 3 &&
            x.MaxCapacity == 10 &&
            x.Description == "new" &&
            x.BasePoints == 7 &&
            x.Enabled == true
        )), Times.Once);
        _attrRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Update_Changes_Enabled_When_Provided()
    {
        // arrange
        var id = Guid.NewGuid();
        var a = new Domain.Attraction
        {
            Id = id, Name = "A", Type = Domain.AttractionType.Show,
            MinAge = 0, MaxCapacity = 5, Description = "d", BasePoints = 0, Enabled = true
        };

        _attrRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()))
                 .Returns(a);
        _attrRepo.Setup(r => r.Update(It.IsAny<Domain.Attraction>()))
                 .Returns<Domain.Attraction>(x => x);

        // act
        var updated = _logic.Update(
            id, "A", Domain.AttractionType.Show, 0, 5, "d", 0, enabled: false);

        // assert
        Assert.IsFalse(updated.Enabled);
        _attrRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()), Times.Once);
        _attrRepo.Verify(r => r.Update(It.Is<Domain.Attraction>(x => x.Id == id && x.Enabled == false)), Times.Once);
        _attrRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Update_Throws_When_Attraction_NotFound()
    {
        // arrange
        var id = Guid.NewGuid();
        _attrRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()))
                 .Returns((Domain.Attraction?)null);

        // act
        void Act() => _logic.Update(id, "X", Domain.AttractionType.Show, 0, 1, "d", 0, null);

        // assert
        Assert.ThrowsException<KeyNotFoundException>((Action)Act);
        _attrRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()), Times.Once);
        _attrRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Update_Throws_On_Invalid_Capacity()
    {
        // arrange
        var id = Guid.NewGuid();
        var a = new Domain.Attraction
        {
            Id = id, Name = "A", Type = Domain.AttractionType.Show,
            MinAge = 0, MaxCapacity = 5, Description = "d", BasePoints = 0, Enabled = true
        };
        _attrRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()))
                 .Returns(a);

        // act
        Action act = () => _logic.Update(id, "A", Domain.AttractionType.Show, 0, 0, "d", 0, null);

        // assert
        Assert.ThrowsException<ArgumentException>(act);
        _attrRepo.Verify(r => r.Find(It.IsAny<Expression<Func<Domain.Attraction, bool>>>()), Times.Never);
        _attrRepo.VerifyNoOtherCalls();
    }

        [TestMethod]
    public void Create_InvalidType_Throws()
    {
        var invalidType = (Domain.AttractionType)999;
        Assert.ThrowsException<ArgumentException>(() =>
            _logic.Create("X", invalidType, 10, 10, "d", 0));
    }

    [TestMethod]
    public void Create_EmptyName_Throws()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            _logic.Create(string.Empty, Domain.AttractionType.Simulator, 0, 10, "desc", 0));
    }

    [TestMethod]
    public void Create_EmptyDescription_Throws()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            _logic.Create("Name", Domain.AttractionType.Simulator, 0, 10, string.Empty, 0));
    }

    [TestMethod]
    public void Delete_Throws_When_NotFound()
    {
        var id = Guid.NewGuid();
        _attrRepo.Setup(r => r.Find(Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>>>()))
                 .Returns((Domain.Attraction?)null);

        Assert.ThrowsException<KeyNotFoundException>(() => _logic.Delete(id));

        _attrRepo.Verify(r => r.Find(Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>>>()), Times.Once);
        _incidentLogic.Verify(x => x.HasActiveIncidents(Moq.It.IsAny<Guid>()), Times.Never);
        _eventLogic.Verify(x => x.IsAttractionReferenced(Moq.It.IsAny<Guid>()), Times.Never);
        _attrRepo.Verify(r => r.Delete(Moq.It.IsAny<Guid>()), Times.Never);
    }

    [TestMethod]
    public void Delete_Succeeds_When_No_Incidents_And_Not_Referenced()
    {
        var id = Guid.NewGuid();
        var a = new Domain.Attraction { Id = id, Name = "A", Type = Domain.AttractionType.Show, MinAge = 0, MaxCapacity = 5, Description = "d", BasePoints = 0, Enabled = true };

        _attrRepo.Setup(r => r.Find(Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>>>()))
                 .Returns(a);
        _incidentLogic.Setup(x => x.HasActiveIncidents(id)).Returns(false);
        _eventLogic.Setup(x => x.IsAttractionReferenced(id)).Returns(false);

        _logic.Delete(id);

        _attrRepo.Verify(r => r.Find(Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>>>()), Times.Once);
        _incidentLogic.Verify(x => x.HasActiveIncidents(id), Times.Once);
        _eventLogic.Verify(x => x.IsAttractionReferenced(id), Times.Once);
        _attrRepo.Verify(r => r.Delete(id), Times.Once);
        _attrRepo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void List_ByType_Only_Returns_Matching_Items()
    {
        var items = new List<Domain.Attraction>
        {
            new Domain.Attraction("RC1", Domain.AttractionType.RollerCoaster, 12, 10, "d", 0){ Enabled=true },
            new Domain.Attraction("RC2", Domain.AttractionType.RollerCoaster, 10, 12, "d", 0){ Enabled=false },
            new Domain.Attraction("SIM1", Domain.AttractionType.Simulator, 8, 9, "d", 0){ Enabled=true }
        };

        _attrRepo
            .Setup(r => r.FindAll(
                Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>>>(),
                Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, object>>[]>()))
            .Returns((System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>> filter,
                      System.Linq.Expressions.Expression<Func<Domain.Attraction, object>>[] _) =>
                     items.Where(filter.Compile()).ToList());

        var res = _logic.List(Domain.AttractionType.RollerCoaster, null).ToList();

        Assert.AreEqual(2, res.Count);
        res.Select(x => x.Name).Should().BeEquivalentTo(new[] { "RC1", "RC2" });
    }

    [TestMethod]
    public void List_ByEnabled_Only_Returns_Matching_Items()
    {
        var items = new List<Domain.Attraction>
        {
            new Domain.Attraction("A", Domain.AttractionType.RollerCoaster, 12, 10, "d", 0){ Enabled=true },
            new Domain.Attraction("B", Domain.AttractionType.Simulator, 10, 12, "d", 0){ Enabled=false },
            new Domain.Attraction("C", Domain.AttractionType.Show, 8, 9, "d", 0){ Enabled=true }
        };

        _attrRepo
            .Setup(r => r.FindAll(
                Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>>>(),
                Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, object>>[]>()))
            .Returns((System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>> filter,
                      System.Linq.Expressions.Expression<Func<Domain.Attraction, object>>[] _) =>
                     items.Where(filter.Compile()).ToList());

        var res = _logic.List(null, true).ToList();

        Assert.AreEqual(2, res.Count);
        res.Should().OnlyContain(x => x.Enabled);
    }

    [TestMethod]
    public void List_NoFilters_Returns_All_Items()
    {
        var items = new List<Domain.Attraction>
        {
            new Domain.Attraction("A", Domain.AttractionType.RollerCoaster,12,10,"d",0){ Enabled=true },
            new Domain.Attraction("B", Domain.AttractionType.Simulator,10,10,"d",0){ Enabled=false },
            new Domain.Attraction("C", Domain.AttractionType.Show,8,5,"d",0){ Enabled=true }
        };

        _attrRepo
            .Setup(r => r.FindAll(
                Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>>>(),
                Moq.It.IsAny<System.Linq.Expressions.Expression<Func<Domain.Attraction, object>>[]>()))
            .Returns((System.Linq.Expressions.Expression<Func<Domain.Attraction, bool>> filter,
                      System.Linq.Expressions.Expression<Func<Domain.Attraction, object>>[] _) =>
                     items.Where(filter.Compile()).ToList());

        var res = _logic.List().ToList();

        Assert.AreEqual(3, res.Count);
        res.Select(x => x.Name).Should().BeEquivalentTo(new[] { "A", "B", "C" });
    }
}

[TestClass]
public class AttractionHelperLogicTest
{
    private Mock<IRepository<Attraction>> _repo = null!;
    private AttractionHelperLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new Mock<IRepository<Attraction>>(MockBehavior.Strict);
        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()))
            .Returns((Attraction?)null);

        _logic = new AttractionHelperLogic(_repo.Object);
    }

    [TestMethod]
    public void GetOrThrow_Returns_Attraction_When_Found()
    {
        // arrange
        var id = Guid.NewGuid();
        var a = new Attraction
        {
            Id = id,
            Name = "Coaster",
            Type = AttractionType.RollerCoaster,
            MinAge = 0,
            MaxCapacity = 20,
            Description = "Fast",
            BasePoints = 7,
            Enabled = true
        };

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()))
            .Returns((Expression<Func<Attraction, bool>> pred) =>
                pred.Compile()(a) ? a : null);

        // act
        var result = _logic.GetOrThrow(id);

        // assert
        result.Should().BeSameAs(a);
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetOrThrow_Throws_When_NotFound()
    {
        // arrange
        var id = Guid.NewGuid();
        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()))
            .Returns((Attraction?)null);

        // act
        Action act = () => _logic.GetOrThrow(id);

        // assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage($"*{id}*");
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<Attraction, bool>>>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }
}
