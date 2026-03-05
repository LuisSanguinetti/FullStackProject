using System.Linq.Expressions;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class ScoringStrategyQueryLogicTest
{
    private Mock<IRepository<ScoringStrategyMeta>> _repo = null!;
    private IScoringStrategyQueryLogic _logic = null!;

    [TestInitialize]
    public void Setup()
    {
        _repo = new Mock<IRepository<ScoringStrategyMeta>>(MockBehavior.Strict);
        _logic = new ScoringStrategyQueryLogic(_repo.Object);
    }

    [TestMethod]
    public void GetActiveOrThrow_ReturnsActiveStrategy_WhenExists()
    {
        // arrange
        var strategy = new ScoringStrategyMeta
        {
            Id = Guid.NewGuid(),
            Name = "Test",
            IsActive = true,             IsDeleted = false,
            FilePath = "test/path",
            FileName = "test.dll"
        };

        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>()))
            .Returns((Expression<Func<ScoringStrategyMeta, bool>> pred) =>
                pred.Compile()(strategy) ? strategy : null);

        // act
        var result = _logic.GetActiveOrThrow();

        // assert
        result.Should().BeSameAs(strategy);
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>()), Times.Once);
    }

    [TestMethod]
    public void GetActiveOrThrow_ThrowsInvalidOperation_WhenNoActiveStrategy()
    {
        // arrange
        _repo.Setup(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>()))
            .Returns((ScoringStrategyMeta?)null);

        // act
        var act = () => _logic.GetActiveOrThrow();

        // assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No active scoring strategy configured");
        _repo.Verify(r => r.Find(It.IsAny<Expression<Func<ScoringStrategyMeta, bool>>>()), Times.Once);
    }
}
