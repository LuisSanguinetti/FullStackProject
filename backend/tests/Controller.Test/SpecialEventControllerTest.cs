using System;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;

[TestClass]
public class SpecialEventControllerTest
{
    [TestMethod]
    public void Create_Returns201()
    {
        var mock = new Mock<ISpecialEventAdminLogic>();
        mock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<Guid[]>()))
            .Returns(new Domain.SpecialEvent { Id = Guid.NewGuid(), Name = "X", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(3), Capacity = 10, ExtraPrice = 150m });

        var ctl = new SpecialEventAdminController(mock.Object);
        var dto = new SpecialEventCreateDto { Name = "Noche", Start = "2025-01-02T20:00:00Z", End = "2025-01-02T23:00:00Z", Capacity = 200, ExtraPrice = 150, AttractionIds = Array.Empty<Guid>() };

        var res = ctl.Create(dto) as CreatedAtActionResult;
        Assert.IsNotNull(res);
    }

    [TestMethod]
    public void Create_InvalidModelState_Returns400()
    {
        var mock = new Mock<ISpecialEventAdminLogic>();
        var ctl = new SpecialEventAdminController(mock.Object);
        ctl.ModelState.AddModelError("Name", "Required");

        var dto = new SpecialEventCreateDto { Name = string.Empty, Start = "2025-01-02T20:00:00Z", End = "2025-01-02T23:00:00Z", Capacity = 200, ExtraPrice = 150, AttractionIds = Array.Empty<Guid>() };

        var res = ctl.Create(dto);
        Assert.IsInstanceOfType(res, typeof(ObjectResult));
        var objResult = (ObjectResult)res;
        Assert.IsInstanceOfType(objResult.Value, typeof(ValidationProblemDetails));
    }

    [TestMethod]
    public void Create_InvalidDateFormat_ReturnsBadRequest()
    {
        var mock = new Mock<ISpecialEventAdminLogic>();
        var ctl = new SpecialEventAdminController(mock.Object);
        var dto = new SpecialEventCreateDto { Name = "Event", Start = "not-a-date", End = "also-not-a-date", Capacity = 200, ExtraPrice = 150, AttractionIds = Array.Empty<Guid>() };

        var res = ctl.Create(dto) as BadRequestObjectResult;
        Assert.IsNotNull(res);
        Assert.AreEqual("Invalid date format", res.Value);
    }

    [TestMethod]
    public void Create_ArgumentException_ReturnsBadRequest()
    {
        var mock = new Mock<ISpecialEventAdminLogic>();
        mock.Setup(m => m.Create(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<Guid[]>()))
            .Throws(new ArgumentException("Attractions missing"));

        var ctl = new SpecialEventAdminController(mock.Object);
        var dto = new SpecialEventCreateDto { Name = "Event", Start = "2025-01-02T20:00:00Z", End = "2025-01-02T23:00:00Z", Capacity = 200, ExtraPrice = 150, AttractionIds = new[] { Guid.NewGuid() } };

        var res = ctl.Create(dto) as BadRequestObjectResult;
        Assert.IsNotNull(res);
    }

    [TestMethod]
    public void GetById_Found_ReturnsEvent()
    {
        var id = Guid.NewGuid();
        var ev = new Domain.SpecialEvent { Id = id, Name = "Event", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(3), Capacity = 10, ExtraPrice = 150m };

        var mock = new Mock<ISpecialEventAdminLogic>();
        mock.Setup(m => m.List()).Returns(new[] { ev });

        var ctl = new SpecialEventAdminController(mock.Object);
        var res = ctl.GetById(id);

        Assert.IsNotNull(res.Value);
        Assert.AreEqual(id, res.Value.Id);
    }

    [TestMethod]
    public void GetById_NotFound_Returns404()
    {
        var mock = new Mock<ISpecialEventAdminLogic>();
        mock.Setup(m => m.List()).Returns(new Domain.SpecialEvent[0]);

        var ctl = new SpecialEventAdminController(mock.Object);
        var res = ctl.GetById(Guid.NewGuid());

        Assert.IsInstanceOfType(res.Result, typeof(NotFoundResult));
    }

    [TestMethod]
    public void Delete_Success_Returns204()
    {
        var id = Guid.NewGuid();
        var mock = new Mock<ISpecialEventAdminLogic>();
        mock.Setup(m => m.Delete(id));

        var ctl = new SpecialEventAdminController(mock.Object);
        var res = ctl.Delete(id) as NoContentResult;

        Assert.IsNotNull(res);
    }

    [TestMethod]
    public void Delete_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        var mock = new Mock<ISpecialEventAdminLogic>();
        mock.Setup(m => m.Delete(id)).Throws<KeyNotFoundException>();

        var ctl = new SpecialEventAdminController(mock.Object);
        var res = ctl.Delete(id) as NotFoundResult;

        Assert.IsNotNull(res);
    }

    [TestMethod]
    public void Delete_InvalidOperation_Returns409()
    {
        var id = Guid.NewGuid();
        var mock = new Mock<ISpecialEventAdminLogic>();
        mock.Setup(m => m.Delete(id)).Throws(new InvalidOperationException("Has tickets"));

        var ctl = new SpecialEventAdminController(mock.Object);
        var res = ctl.Delete(id) as ConflictObjectResult;

        Assert.IsNotNull(res);
    }

    [TestMethod]
    public void List_ReturnsEvents()
    {
        var events = new[]
        {
            new Domain.SpecialEvent { Id = Guid.NewGuid(), Name = "E1", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(1), Capacity = 10, ExtraPrice = 50m },
            new Domain.SpecialEvent { Id = Guid.NewGuid(), Name = "E2", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Capacity = 20, ExtraPrice = 100m }
        };

        var mock = new Mock<ISpecialEventAdminLogic>();
        mock.Setup(m => m.List()).Returns(events);

        var ctl = new SpecialEventAdminController(mock.Object);
        var res = ctl.List().ToList();

        Assert.AreEqual(2, res.Count);
    }
}
