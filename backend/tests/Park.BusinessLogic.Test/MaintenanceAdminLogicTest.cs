using System;
using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Park.BusinessLogic;

namespace Park.BusinessLogic.Test;

[TestClass]
public class MaintenanceAdminLogicTests
{
    [TestMethod]
    public void Schedule_AddsMaintenance_AndReportsIncident()
    {
        var attr = new Attraction { Id = Guid.NewGuid(), Name = "A", Type = AttractionType.RollerCoaster, MinAge = 0, MaxCapacity = 10, Description = "d", BasePoints = 1 };

        var attractions = new Mock<IAttractionLogic>();
        attractions.Setup(a => a.GetOrThrow(attr.Id)).Returns(attr);

        var incidents = new Mock<IIncidentLogic>();
        var repo = new Mock<IRepository<Maintenance>>();

        var sut = new MaintenanceAdminLogic(attractions.Object, incidents.Object, repo.Object);

        var start = DateTime.UtcNow.AddHours(1);
        var id = sut.Schedule(attr.Id, start, 60, "Cambio de piezas");

        repo.Verify(r => r.Add(It.Is<Maintenance>(m =>
            m.AttractionId == attr.Id &&
            m.StartAt == start &&
            m.DurationMinutes == 60 &&
            m.Description.Contains("Cambio"))), Times.Once);

        incidents.Verify(i => i.CreateIncident(It.Is<string>(s => s.Contains("Maintenance window")), start, attr.Id), Times.Once);
    }

    [TestMethod]
    public void Cancel_UpdatesMaintenance_AndResolvesIncident()
    {
        var attrId = Guid.NewGuid();
        var start = DateTime.UtcNow.AddHours(1);

        var m = new Maintenance(attrId, new Attraction
        {
            Id = attrId,
            Name = "A",
            Type = AttractionType.RollerCoaster,
            MinAge = 0,
            MaxCapacity = 10,
            Description = "d",
            BasePoints = 1
        }, start, 60, "desc");

        var attractions = new Mock<IAttractionLogic>();
        var incidents = new Mock<IIncidentLogic>();
        var repo = new Mock<IRepository<Maintenance>>();
        repo.Setup(r => r.Find(It.IsAny<System.Linq.Expressions.Expression<Func<Maintenance, bool>>>())).Returns(m);

        var sut = new MaintenanceAdminLogic(attractions.Object, incidents.Object, repo.Object);
        var when = start.AddMinutes(10);
        incidents.Setup(i => i.HasActiveIncidents(attrId)).Returns(true);
        var inc = new Incident("auto", start, m.Attraction, attrId);
        incidents.Setup(i => i.GetByAttractionIdOrThrow(attrId)).Returns(inc);
        incidents.Setup(i => i.ResolveIncident(inc.Id));

        sut.Cancel(m.Id, when);

        repo.Verify(r => r.Update(It.Is<Maintenance>(x => x.Cancelled)), Times.Once);
        incidents.Verify(i => i.ResolveIncident(It.IsAny<Guid>()), Times.Once);
    }
}
