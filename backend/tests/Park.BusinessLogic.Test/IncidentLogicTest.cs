using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class IncidentLogicTest
{
    private Mock<IRepository<Incident>> _repo = null!;
    private Mock<IAttractionHelperLogic> _attractionLogic = null!;
    private IncidentLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new Mock<IRepository<Incident>>(MockBehavior.Strict);
        _attractionLogic = new Mock<IAttractionHelperLogic>(MockBehavior.Strict);
        _logic = new IncidentLogic(_repo.Object, _attractionLogic.Object);
    }

    [TestMethod]
    public void GetByAttractionIdOrThrow()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test",
            MinAge = 12,
            MaxCapacity = 100,
            Description = "Test",
            BasePoints = 0
        };

        var incidentId = Guid.NewGuid();
        var incident = new Incident
        {
            Id = incidentId,
            AttractionId = attractionId,
            Attraction = attraction,
            Description = "Test",
            ReportedAt = DateTime.UtcNow
        };

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Incident, bool>>>()))
      .Returns((Expression<Func<Incident, bool>> pred) =>
          pred.Compile()(incident) ? incident : null);

        // Act
        var result = _logic.GetByAttractionIdOrThrow(attractionId);

        // Assert
        result.Should().BeSameAs(incident);
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<Incident, bool>>>()), Times.Once);
        _repo.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void ValidateIncidentTest()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test",
            MinAge = 12,
            MaxCapacity = 100,
            Description = "Test",
            BasePoints = 0
        };

        var incidentId = Guid.NewGuid();
        var incident = new Incident
        {
            Id = incidentId,
            AttractionId = attractionId,
            Attraction = attraction,
            Description = "Test",
            ReportedAt = DateTime.UtcNow
        };

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Incident, bool>>>()))
      .Returns((Expression<Func<Incident, bool>> pred) =>
          pred.Compile()(incident) ? incident : null);

        // Act
        var result = _logic.ValidateIncident(attractionId);

        // Assert
        Assert.AreEqual(false, result);
    }

    [TestMethod]
    public void CreateIncidentTest()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var attraction = new Attraction
        {
            Id = attractionId,
            Name = "Test Ride",
            MinAge = 10,
            MaxCapacity = 50,
            Description = "Test",
            BasePoints = 0
        };

        var description = "Test";
        var reportedAt = DateTime.UtcNow;

        _attractionLogic
            .Setup(a => a.GetOrThrow(attractionId))
            .Returns(attraction);

        _repo
         .Setup(r => r.Add(It.IsAny<Incident>()))
         .Returns((Incident i) => i);

        // Act
        _logic.CreateIncident(description, reportedAt, attractionId);

        // Assert
        _repo.Verify(r => r.Add(It.Is<Incident>(i =>
            i.Description == description &&
            i.ReportedAt == reportedAt &&
            i.AttractionId == attractionId &&
            i.Attraction == attraction
        )), Times.Once);
    }

    [TestMethod]
    public void ResolveIncidentTest()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var attraction = new Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Test Ride",
            MinAge = 10,
            MaxCapacity = 5,
            Description = "Test"
        };

        var incident = new Incident(
            description: "Some incident",
            reportedAt: DateTime.UtcNow,
            attraction: attraction,
            attractionId: attraction.Id
        );

        incident.Id = incidentId;

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<Incident, bool>>>()))
             .Returns((Expression<Func<Incident, bool>> pred) => pred.Compile()(incident) ? incident : null);

        _repo.Setup(r => r.Update(It.IsAny<Incident>()))
             .Returns((Incident i) => i);

        // Act
        _logic.ResolveIncident(incidentId);

        // Assert
        Assert.IsTrue(incident.Resolved);
        _repo.Verify(r => r.Update(It.IsAny<Incident>()), Times.Once);
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<Incident, bool>>>()), Times.Once);
    }

    [TestMethod]
    public void HasActiveIncidents_ReturnsTrue_When_ActiveExists()
    {
        var attractionId = Guid.NewGuid();

        _repo.Setup(r => r.FindAll(
                It.IsAny<Expression<Func<Incident, bool>>>(),
                Array.Empty<Expression<Func<Incident, object>>>()))
            .Returns(new List<Incident>
            {
                    new Incident("desc", DateTime.UtcNow,
                        new Attraction { Id = attractionId, Name = "A", Description = "d", Type = AttractionType.Simulator, MinAge = 0, MaxCapacity = 1 },
                        attractionId) { Resolved = false }
            });

        var result = _logic.HasActiveIncidents(attractionId);

        Assert.IsTrue(result);
        _repo.VerifyAll();
    }

    [TestMethod]
    public void HasActiveIncidents_ReturnsFalse_When_NoneActive()
    {
        var attractionId = Guid.NewGuid();

        _repo.Setup(r => r.FindAll(
                It.IsAny<Expression<Func<Incident, bool>>>(),
                Array.Empty<Expression<Func<Incident, object>>>()))
            .Returns(new List<Incident>()); // empty

        var result = _logic.HasActiveIncidents(attractionId);

        Assert.IsFalse(result);
        _repo.VerifyAll();
    }

    [TestMethod]
    public void HasActiveIncidents_ReturnsFalse_When_RepoReturnsNull()
    {
        var attractionId = Guid.NewGuid();

        _repo.Setup(r => r.FindAll(
                It.IsAny<Expression<Func<Incident, bool>>>(),
                Array.Empty<Expression<Func<Incident, object>>>()))
            .Returns(() => (IList<Incident>?)null);

        var result = _logic.HasActiveIncidents(attractionId);

        Assert.IsFalse(result);
        _repo.VerifyAll();
    }

    [TestMethod]
    public void GetAllIncident()
    {
        // Arrange
        var attraction = new Attraction
        {
            Id = Guid.NewGuid(),
            Name = "Ride",
            MinAge = 10,
            MaxCapacity = 50,
            Description = "Desc"
        };

        var incidents = new List<Incident>
    {
        new Incident("Desc1", DateTime.UtcNow, attraction, attraction.Id),
        new Incident("Desc2", DateTime.UtcNow, attraction, attraction.Id)
    };

        _repo.Setup(r => r.FindAll())
             .Returns(incidents);

        // Act
        var result = _logic.GetAllIncident();

        // Assert
        result.Should().BeEquivalentTo(incidents);
        _repo.Verify(r => r.FindAll(), Times.Once);
        _repo.VerifyNoOtherCalls();
    }
}