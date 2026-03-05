using IParkBusinessLogic;
using Moq;
using obligatorio.WebApi.Controllers;

namespace Controller.Test;

[TestClass]
public class AttractionControllerTest
{
    private Mock<IAttractionLogic> _logicMock = null!;
    private AttractionController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logicMock = new Mock<IAttractionLogic>(MockBehavior.Strict);
        _controller = new AttractionController(_logicMock.Object);
    }

    [TestMethod]
    public void NumberOfVisits_ControllerTest()
    {
        // Arrange
        var attractionId = Guid.NewGuid();
        var startDate = new DateTime(2025, 1, 1);
        var endDate   = new DateTime(2025, 1, 10);
        var expectedCount = 2;

        _logicMock
            .Setup(l => l.NumberOfVisits(attractionId, startDate, endDate))
            .Returns(expectedCount);

        // Act
        var action = _controller.GetVisitorQuantity(attractionId, startDate, endDate);

        // Assert
        Assert.AreEqual(expectedCount, action.Value);
        _logicMock.Verify(l => l.NumberOfVisits(attractionId, startDate, endDate), Times.Once);
        _logicMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void AdminCreate_Returns201()
    {
        var mock = new Mock<IParkBusinessLogic.IAttractionAdminLogic>(MockBehavior.Strict);
        var expected = new Domain.Attraction("A", Domain.AttractionType.RollerCoaster, 10, 20, "d", 5)
            { Id = Guid.NewGuid(), Enabled = true };

        mock.Setup(m => m.Create("A", Domain.AttractionType.RollerCoaster, 10, 20, "d", 5))
            .Returns(expected);

        var ctl = new obligatorio.WebApi.Controllers.AttractionAdminController(mock.Object);
        var dto = new obligatorio.WebApi.DTO.AttractionCreateDto { Name = "A", Type = "RollerCoaster", MinAge = 10, Capacity = 20, Description = "d", BasePoints = 5 };

        var res = ctl.Create(dto) as Microsoft.AspNetCore.Mvc.CreatedAtActionResult;
        Assert.IsNotNull(res);
        var body = (obligatorio.WebApi.DTO.AttractionGetDto)res.Value!;
        Assert.AreEqual(expected.Id, body.Id);

        mock.VerifyAll();
    }

    [TestMethod]
    public void AdminCreate_Throws_WhenInvalidType()
    {
        var mock = new Mock<IParkBusinessLogic.IAttractionAdminLogic>(MockBehavior.Strict);
        var ctl = new obligatorio.WebApi.Controllers.AttractionAdminController(mock.Object);
        var dto = new obligatorio.WebApi.DTO.AttractionCreateDto { Name = "A", Type = "NOPE", MinAge = 0, Capacity = 10, Description = "d" };

        Assert.ThrowsException<ArgumentException>(() => ctl.Create(dto));
        mock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void AdminCreate_Throws_WhenCapacityInvalid()
    {
        var mock = new Mock<IParkBusinessLogic.IAttractionAdminLogic>(MockBehavior.Strict);
        mock.Setup(m => m.Create("A", Domain.AttractionType.RollerCoaster, 0, 0, "d", 0))
            .Throws(new ArgumentException("Capacity must be > 0", "capacity"));

        var ctl = new obligatorio.WebApi.Controllers.AttractionAdminController(mock.Object);
        var dto = new obligatorio.WebApi.DTO.AttractionCreateDto { Name = "A", Type = "RollerCoaster", MinAge = 0, Capacity = 0, Description = "d", BasePoints = 0 };

        Assert.ThrowsException<ArgumentException>(() => ctl.Create(dto));
        mock.VerifyAll();
    }
}
