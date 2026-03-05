using System;
using System.Collections.Generic;
using Domain;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;

[TestClass]
public class MaintenancesControllerTests
{
    [TestMethod]
    public async Task Create_ReturnsCreatedAt_WithId()
    {
        var admin = new Mock<IMaintenanceAdminLogic>();
        var query = new Mock<IMaintenanceQueryLogic>();
        var id = Guid.NewGuid();
        admin.Setup(a => a.ScheduleAsync(
It.IsAny<Guid>(),
    It.IsAny<DateTime>(),
It.IsAny<int>(),
    It.IsAny<string>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(id);
        var ctrl = new MaintenancesController(admin.Object, query.Object);
        var result = await ctrl.Create(new obligatorio.WebApi.DTO.MaintenanceCreateDto
        {
            AttractionId = Guid.NewGuid(),
            StartAtUtc = DateTime.UtcNow.AddHours(1),
            DurationMinutes = 60,
            Description = "desc"
        }) as CreatedAtActionResult;

        Assert.IsNotNull(result);
        Assert.AreEqual("GetById", result!.ActionName);
        Assert.AreEqual(id, (Guid)result!.RouteValues!["id"]);
    }

    [TestMethod]
    public void List_ReturnsOk_WithMappedDtos()
    {
        var admin = new Mock<IMaintenanceAdminLogic>();
        var query = new Mock<IMaintenanceQueryLogic>();

        var attr = new Attraction { Id = Guid.NewGuid(), Name = "Atrac", Type = AttractionType.RollerCoaster, MinAge = 0, MaxCapacity = 10, Description = "d", BasePoints = 1 };
        var m = new Maintenance(attr.Id, attr, DateTime.UtcNow.AddHours(1), 30, "desc");

        query.Setup(q => q.List(null, null, null)).Returns(new List<Maintenance> { m });

        var ctrl = new MaintenancesController(admin.Object, query.Object);
        var result = ctrl.List(null, null, null) as OkObjectResult;

        Assert.IsNotNull(result);
        var list = result!.Value as IEnumerable<object>;
        Assert.IsNotNull(list);
    }

    [TestMethod]
    public async Task Create_CallsAdmin_WithDtoValues()
    {
        var admin = new Mock<IMaintenanceAdminLogic>(MockBehavior.Strict);
        var query = new Mock<IMaintenanceQueryLogic>();
        var start = new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        var dto = new MaintenanceCreateDto
        {
            AttractionId = Guid.NewGuid(),
            StartAtUtc = start,
            DurationMinutes = 90,
            Description = "descX"
        };

        admin.Setup(a => a.ScheduleAsync(dto.AttractionId, dto.StartAtUtc, dto.DurationMinutes, dto.Description, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Guid.NewGuid())
             .Verifiable();

        var ctrl = new MaintenancesController(admin.Object, query.Object);
        await ctrl.Create(dto);

        admin.Verify(a => a.ScheduleAsync(dto.AttractionId, dto.StartAtUtc, dto.DurationMinutes, dto.Description, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task Cancel_ReturnsNoContent_AndCallsLogic()
    {
        var admin = new Mock<IMaintenanceAdminLogic>(MockBehavior.Strict);
        var query = new Mock<IMaintenanceQueryLogic>();
        var id = Guid.NewGuid();

        admin.Setup(a => a.CancelAsync(id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var ctrl = new MaintenancesController(admin.Object, query.Object);
        var res = await ctrl.Cancel(id) as NoContentResult;

        Assert.IsNotNull(res);
        admin.Verify(a => a.CancelAsync(id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public void GetById_ReturnsOk_WithMappedDto()
    {
        var admin = new Mock<IMaintenanceAdminLogic>();
        var query = new Mock<IMaintenanceQueryLogic>();

        var attr = new Attraction { Id = Guid.NewGuid(), Name = "Atrac", Type = AttractionType.RollerCoaster, MinAge = 0, MaxCapacity = 10, Description = "d", BasePoints = 1 };
        var start = new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        var m = new Maintenance(attr.Id, attr, start, 45, "descX");

        query.Setup(q => q.List(null, null, null)).Returns(new List<Maintenance> { m });

        var ctrl = new MaintenancesController(admin.Object, query.Object);
        var ok = ctrl.GetById(m.Id) as OkObjectResult;

        Assert.IsNotNull(ok);
        var dto = ok!.Value as MaintenanceDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual(m.Id, dto!.Id);
        Assert.AreEqual(attr.Id, dto.AttractionId);
        Assert.AreEqual(attr.Name, dto.AttractionName);
        Assert.AreEqual(m.StartAt, dto.StartAtUtc);
        Assert.AreEqual(m.DurationMinutes, dto.DurationMinutes);
        Assert.AreEqual(m.EndAt, dto.EndAtUtc);
        Assert.AreEqual(m.Description, dto.Description);
        Assert.AreEqual(m.Cancelled, dto.Cancelled);
    }

    [TestMethod]
    public void GetById_ReturnsNotFound_WhenMissing()
    {
        var admin = new Mock<IMaintenanceAdminLogic>();
        var query = new Mock<IMaintenanceQueryLogic>();
        query.Setup(q => q.List(null, null, null)).Returns(new List<Maintenance>());

        var ctrl = new MaintenancesController(admin.Object, query.Object);
        var res = ctrl.GetById(Guid.NewGuid());

        Assert.IsInstanceOfType(res, typeof(NotFoundResult));
    }

    [TestMethod]
    public void List_WithFilters_ForwardsParams_AndMaps()
    {
        var admin = new Mock<IMaintenanceAdminLogic>();
        var query = new Mock<IMaintenanceQueryLogic>();

        var attr = new Attraction { Id = Guid.NewGuid(), Name = "A1", Type = AttractionType.RollerCoaster, MinAge = 0, MaxCapacity = 10, Description = "d", BasePoints = 1 };
        var from = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to   = new DateTime(2025, 1, 3, 0, 0, 0, DateTimeKind.Utc);
        var start = new DateTime(2025, 1, 2, 10, 0, 0, DateTimeKind.Utc);
        var m = new Maintenance(attr.Id, attr, start, 30, "desc");

        query.Setup(q => q.List(attr.Id, from, to)).Returns(new List<Maintenance> { m });

        var ctrl = new MaintenancesController(admin.Object, query.Object);
        var ok = ctrl.List(attr.Id, from, to) as OkObjectResult;

        Assert.IsNotNull(ok);
        var list = (ok!.Value as IEnumerable<MaintenanceDto>)!.ToList();
        Assert.AreEqual(1, list.Count);
        var dto = list[0];
        Assert.AreEqual(m.Id, dto.Id);
        Assert.AreEqual(attr.Id, dto.AttractionId);
        Assert.AreEqual(attr.Name, dto.AttractionName);
        Assert.AreEqual(start, dto.StartAtUtc);
        Assert.AreEqual(30, dto.DurationMinutes);
        Assert.AreEqual(start.AddMinutes(30), dto.EndAtUtc);
        Assert.AreEqual("desc", dto.Description);
        Assert.AreEqual(false, dto.Cancelled);

        query.Verify(q => q.List(attr.Id, from, to), Times.Once);
    }
}
