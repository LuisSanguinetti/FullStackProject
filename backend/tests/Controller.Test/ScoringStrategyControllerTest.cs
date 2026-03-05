using Domain;
using FluentAssertions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;

namespace Controller.Test;

[TestClass]
public class ScoringStrategyControllerTest
{
    private Mock<IScoringStrategyMetaLogic> _logic = null!;
    private ScoringStrategyController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _logic = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        _controller = new ScoringStrategyController(_logic.Object);
    }

    [TestMethod]
    public async Task GetAll_ReturnsOk_WithItems()
    {
        // Arrange
        var items = new List<ScoringStrategyMeta>
        {
            new ScoringStrategyMeta
            {
                Id = Guid.NewGuid(),
                Name = "one",
                FileName = "one.dll",
                FilePath = "/path/one.dll",
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            },
            new ScoringStrategyMeta
            {
                Id = Guid.NewGuid(),
                Name = "two",
                FileName = "two.dll",
                FilePath = "/path/two.dll",
                IsActive = false,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow.AddMinutes(-1)
            }
        };

        var mock = new Mock<IScoringStrategyMetaLogic>();
        mock.Setup(m => m.ListAsync(false)).ReturnsAsync(items);

        var controller = new ScoringStrategyController(mock.Object);

        // Act
        var result = await controller.GetAll();

        // Assert
        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok, "Expected OkObjectResult");
        Assert.AreEqual(200, ok.StatusCode ?? 200);

        // basic shape check
        var list = ok.Value as IEnumerable<object>;
        Assert.IsNotNull(list);

        mock.Verify(m => m.ListAsync(false), Times.Once);
    }

    [TestMethod]
    public async Task Activate_ReturnsNoContent_AndCallsLogic()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mock = new Mock<IScoringStrategyMetaLogic>();
        mock.Setup(m => m.ActivateAsync(id)).Returns(Task.CompletedTask);

        var controller = new ScoringStrategyController(mock.Object);

        // Act
        var result = await controller.Activate(id);

        // Assert
        var noContent = result as NoContentResult;
        Assert.IsNotNull(noContent, "Expected NoContentResult");
        Assert.AreEqual(204, noContent.StatusCode);

        mock.Verify(m => m.ActivateAsync(id), Times.Once);
    }

    [TestMethod]
    public async Task SoftDelete_ReturnsNoContent_AndCallsLogic()
    {
        // Arrange
        var id = Guid.NewGuid();
        var mock = new Mock<IScoringStrategyMetaLogic>();
        mock.Setup(m => m.SoftDeleteAsync(id)).Returns(Task.CompletedTask);

        var controller = new ScoringStrategyController(mock.Object);

        // Act
        var result = await controller.SoftDelete(id);

        // Assert
        var noContent = result as NoContentResult;
        Assert.IsNotNull(noContent, "Expected NoContentResult");
        Assert.AreEqual(204, noContent.StatusCode);

        mock.Verify(m => m.SoftDeleteAsync(id), Times.Once);
    }

    [TestMethod]
    public void GetActive_Returns200_WithActiveMeta()
    {
        // arrange
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
        var metas = new List<ScoringStrategyMeta> { meta };

        _logic.Setup(l => l.GetActiveOrThrow()).Returns(metas);

        // act
        var action = _controller.GetActive();

        // assert
        var ok = action as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.StatusCode.Should().Be(200);
        ok.Value.Should().BeEquivalentTo(metas);

        _logic.Verify(l => l.GetActiveOrThrow(), Times.Once);
        _logic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void GetActive_PropagatesException_WhenNoActiveConfigured()
    {
        // arrange
        _logic.Setup(l => l.GetActiveOrThrow())
            .Throws(new InvalidOperationException("No active scoring strategy configured."));

        // act
        Action act = () => _controller.GetActive();

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*No active scoring strategy*");

        _logic.Verify(l => l.GetActiveOrThrow(), Times.Once);
        _logic.VerifyNoOtherCalls();
    }
}
