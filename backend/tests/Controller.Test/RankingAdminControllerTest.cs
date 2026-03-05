using System;
using System.Collections.Generic;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

namespace Controller.Test;

[TestClass]
public class RankingAdminControllerTest
{
    private Mock<IRankingLogic> _rankingMock = null!;
    private RankingAdminController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _rankingMock = new Mock<IRankingLogic>(MockBehavior.Strict);
        _controller  = new RankingAdminController(_rankingMock.Object);
    }

    [TestMethod]
    public void Daily_Returns_Ok_With_List()
    {
        // Arrange
        var e1 = new DailyRankingEntry(
            Guid.NewGuid(), "Alice", "Baker", "alice@x", 123);
        var e2 = new DailyRankingEntry(
            Guid.NewGuid(), "Bob", "Carson", "bob@x", 100);

        _rankingMock.Setup(l => l.GetDailyTop(10))
                    .Returns(new List<DailyRankingEntry> { e1, e2 });

        // Act
        var res = _controller.GetDaily() as OkObjectResult;

        // Assert
        Assert.IsNotNull(res);
        var list = res.Value as IEnumerable<RankingEntryDto>;
        Assert.IsNotNull(list);

        var arr = list!.ToList();
        Assert.AreEqual(2, arr.Count);

        _rankingMock.Verify(l => l.GetDailyTop(10), Times.Once);
        _rankingMock.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void Daily_Maps_To_Dto()
    {
        // Arrange
        var uid = Guid.NewGuid();
        var entry = new DailyRankingEntry(uid, "Alice", "Baker", "alice@x", 77);

        _rankingMock.Setup(l => l.GetDailyTop(10))
                    .Returns(new List<DailyRankingEntry> { entry });

        // Act
        var ok = _controller.GetDaily() as OkObjectResult;

        // Assert
        Assert.IsNotNull(ok);
        var list = ok.Value as IEnumerable<RankingEntryDto>;
        Assert.IsNotNull(list);

        var first = list!.First();
        Assert.AreEqual(uid, first.UserId);
        Assert.AreEqual("Alice", first.Name);
        Assert.AreEqual("Baker", first.Surname);
        Assert.AreEqual("alice@x", first.Email);
        Assert.AreEqual(77, first.TotalPoints);

        _rankingMock.Verify(l => l.GetDailyTop(10), Times.Once);
        _rankingMock.VerifyNoOtherCalls();
    }
}
