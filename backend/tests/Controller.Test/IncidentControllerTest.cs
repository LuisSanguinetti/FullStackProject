using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;

[TestClass]
public class IncidentControllerTest
{
    private Mock<IIncidentLogic> _incidentLogic = null!;
    private IncidentController _incidentController = null!;

    [TestInitialize]
    public void Setup()
    {
        _incidentLogic = new Mock<IIncidentLogic>(MockBehavior.Strict);
        _incidentController = new IncidentController(_incidentLogic.Object);
    }

    [TestMethod]
    public void PostCreateIncidentTest()
    {
        // arrange
        var dto = new CreateIncidentDto
        {
            Description = "Test incident",
            ReportedAt = DateTime.UtcNow,
            AttractionId = Guid.NewGuid()
        };

        _incidentLogic
            .Setup(l => l.CreateIncident(dto.Description, dto.ReportedAt, dto.AttractionId))
            .Verifiable();

        // act
        var result = _incidentController.PostCreateIncident(dto);

        // assert
        var ok = result as OkResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(200, ok!.StatusCode);
        _incidentLogic.Verify(l => l.CreateIncident(dto.Description, dto.ReportedAt, dto.AttractionId), Times.Once);
        _incidentLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void PutResolveIncidentTest()
    {
        // arrange
        var incidentId = Guid.NewGuid();

        _incidentLogic
            .Setup(l => l.ResolveIncident(incidentId))
            .Verifiable();

        // act
        var result = _incidentController.PutResolveIncident(incidentId);

        // assert
        var ok = result as OkResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(200, ok!.StatusCode);
        _incidentLogic.Verify(l => l.ResolveIncident(incidentId), Times.Once);
        _incidentLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetAllIncidentTest()
    {
        // Arrange
        var incidents = new List<Domain.Incident>
    {
        new Domain.Incident("Desc1", DateTime.UtcNow, new Domain.Attraction { Id = Guid.NewGuid(), Name = "Ride1", MinAge = 10, MaxCapacity = 50, Description = "Desc" }, Guid.NewGuid()),
        new Domain.Incident("Desc2", DateTime.UtcNow, new Domain.Attraction { Id = Guid.NewGuid(), Name = "Ride2", MinAge = 12, MaxCapacity = 60, Description = "Desc" }, Guid.NewGuid())
    };

        _incidentLogic
            .Setup(l => l.GetAllIncident())
            .Returns(incidents)
            .Verifiable();

        // Act
        var result = _incidentController.GetAllIncident();

        // Assert
        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(200, ok!.StatusCode);
        Assert.AreEqual(incidents, ok.Value);
        _incidentLogic.Verify(l => l.GetAllIncident(), Times.Once);
        _incidentLogic.VerifyNoOtherCalls();
    }
}