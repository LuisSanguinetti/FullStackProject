using Domain;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;

[TestClass]
public class AttractionAdminControllerTest
{
    private Mock<IAttractionAdminLogic> _logic = null!;
    private AttractionAdminController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logic = new Mock<IAttractionAdminLogic>(MockBehavior.Strict);
        _controller = new AttractionAdminController(_logic.Object);
    }

    [TestMethod]
    public void GetById_Returns_Dto_From_Logic()
    {
        // arrange
        var id = Guid.NewGuid();
        var a = new Attraction("Coaster", AttractionType.RollerCoaster, 12, 24, "desc", 10) { Id = id, Enabled = true };
        _logic.Setup(l => l.GetOrThrow(id)).Returns(a);

        // act
        var result = _controller.GetById(id);

        // assert (ActionResult<T> with direct T => Value is set, Result is null)
        Assert.IsNotNull(result);
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(id, result.Value!.Id);
        Assert.AreEqual("Coaster", result.Value.Name);
        _logic.Verify(l => l.GetOrThrow(id), Times.Once);
        _logic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Update_Parses_Type_And_Returns_Updated_Dto()
    {
        // arrange
        var id = Guid.NewGuid();
        var dto = new AttractionUpdateDto
        {
            Name = "Updated",
            Type = "rollercoaster", // lower-case to cover ignoreCase in Enum.Parse
            MinAge = 15,
            Capacity = 30,
            Description = "new-desc",
            BasePoints = 5,
            Enabled = false
        };

        var returned = new Attraction(dto.Name, AttractionType.RollerCoaster, dto.MinAge, dto.Capacity, dto.Description, dto.BasePoints)
        { Id = id, Enabled = dto.Enabled ?? true };

        _logic.Setup(l => l.Update(
                id,
                dto.Name,
                AttractionType.RollerCoaster,
                dto.MinAge,
                dto.Capacity,
                dto.Description,
                dto.BasePoints,
                dto.Enabled))
              .Returns(returned);

        // act
        var result = _controller.Update(id, dto);

        // assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.Result);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(id, result.Value!.Id);
        Assert.AreEqual("Updated", result.Value.Name);
        Assert.AreEqual("RollerCoaster", result.Value.Type);
        _logic.VerifyAll();
        _logic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Delete_Calls_Logic_And_Returns_NoContent()
    {
        // arrange
        var id = Guid.NewGuid();
        _logic.Setup(l => l.Delete(id));

        // act
        var result = _controller.Delete(id);

        // assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        _logic.Verify(l => l.Delete(id), Times.Once);
        _logic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void List_With_Type_And_Enabled_Parses_And_Maps()
    {
        // arrange
        var items = new List<Attraction>
        {
            new Attraction("RC1", AttractionType.RollerCoaster, 12, 24, "d", 1){ Id = Guid.NewGuid(), Enabled = true },
            new Attraction("RC2", AttractionType.RollerCoaster, 10, 20, "d", 2){ Id = Guid.NewGuid(), Enabled = true },
        };

        _logic.Setup(l => l.List(AttractionType.RollerCoaster, true))
              .Returns(items);

        // act
        var actionResult = _controller.List("rollercoaster", true);
        var okResult = actionResult as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult from List");

        var dtos = okResult.Value as IEnumerable<AttractionGetDto>;
        Assert.IsNotNull(dtos, "Expected IEnumerable<AttractionGetDto>");

        var dtosList = dtos.ToList();

        // assert
        Assert.AreEqual(2, dtosList.Count);
        CollectionAssert.AreEquivalent(
            items.Select(i => i.Name).ToList(),
            dtosList.Select(d => d.Name).ToList()
        );
    }

    [TestMethod]
    public void List_With_No_Filters_Returns_All()
    {
        // arrange
        var items = new List<Attraction>
        {
            new Attraction("A", AttractionType.Show, 0, 10, "d", 0){ Id = Guid.NewGuid(), Enabled = false },
            new Attraction("B", AttractionType.Simulator, 5, 15, "d", 0){ Id = Guid.NewGuid(), Enabled = true },
        };

        _logic.Setup(l => l.List(null, null)).Returns(items);

        // act (both query params null path)
        var actionResult = _controller.List(null, null);
        var okResult = actionResult as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult from List");

        var dtos = okResult.Value as IEnumerable<AttractionGetDto>;
        Assert.IsNotNull(dtos, "Expected IEnumerable<AttractionGetDto>");

        var dtosList = dtos.ToList();

        // assert
        Assert.AreEqual(2, dtosList.Count);
        Assert.IsTrue(dtosList.Any(d => d.Name == "A"));
        Assert.IsTrue(dtosList.Any(d => d.Name == "B"));
        _logic.Verify(l => l.List(null, null), Times.Once);
        _logic.VerifyNoOtherCalls();
    }
}
