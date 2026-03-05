using System;
using System.Collections.Generic;
using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using obligatorio.WebApi.Controllers;
using obligatorio.WebApi.DTO;

[TestClass]
public class PointsControllerTest
{
    [TestMethod]
    public void GetHistory_ReturnsOk_WithDto()
    {
        var user = Guid.NewGuid();
        var sid = Guid.NewGuid();
        var u = new User("Bob", "Carson", "bob@ex.com", "p2", new DateOnly(1991, 2, 2), MembershipLevel.Standard);

        var history = new Mock<IPointsHistoryLogic>();
        history.Setup(h => h.List(user, null, null))
            .Returns(new List<PointsAward>
            {
                new PointsAward { User = u, Id = Guid.NewGuid(), UserId = user, Points = 7, Reason = "Atracción", StrategyId = sid, At = DateTime.UtcNow }
            });

        var strategies = new Mock<IRepository<ScoringStrategyMeta>>();
        strategies.Setup(s => s.FindAll(It.IsAny<System.Linq.Expressions.Expression<Func<ScoringStrategyMeta, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<ScoringStrategyMeta, object>>[]>()))
            .Returns(new List<ScoringStrategyMeta>
            {
                new ScoringStrategyMeta { Id = sid, Name = "Base", FileName = "a", FilePath = "b", IsActive = true, IsDeleted = false, CreatedOn = DateTime.UtcNow }
            });

        var ctrl = new PointsController(history.Object, strategies.Object);

        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        ctrl.ControllerContext.HttpContext.Items["CurrentUserId"] = user;

        var res = ctrl.GetHistory(null, null) as OkObjectResult;

        Assert.IsNotNull(res);
        Assert.IsNotNull(res!.Value);
    }

    [TestMethod]
    public void GetHistory_MapsAllFields_ToDto()
    {
        var u1 = new User("Alice", "Baker", "alice@ex.com", "p1", new DateOnly(1990, 1, 1), MembershipLevel.Standard);
        var sid = Guid.NewGuid();
        var at = new DateTime(2025, 1, 2, 12, 0, 0, DateTimeKind.Utc);

        var history = new Mock<IPointsHistoryLogic>();
        history.Setup(h => h.List(u1.Id, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
               .Returns(new List<PointsAward>
               {
                   new PointsAward
                   {
                       Id = Guid.NewGuid(), User = u1, UserId = u1.Id, Points = 9, Reason = "Access",
                       StrategyId = sid, At = at
                   }
               });

        var strategies = new Mock<IRepository<ScoringStrategyMeta>>();
        strategies.Setup(s => s.FindAll(It.IsAny<System.Linq.Expressions.Expression<Func<ScoringStrategyMeta, bool>>>(),
                                        It.IsAny<System.Linq.Expressions.Expression<Func<ScoringStrategyMeta, object>>[]>()))
                  .Returns(new List<ScoringStrategyMeta>
                  {
                      new ScoringStrategyMeta
                      {
                          Id = sid, Name = "BaseStrategy", FileName = "f", FilePath = "p",
                          IsActive = true, IsDeleted = false, CreatedOn = DateTime.UtcNow
                      }
                  });

        var ctrl = new PointsController(history.Object, strategies.Object);
        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        ctrl.ControllerContext.HttpContext.Items["CurrentUserId"] = u1.Id;

        var res = ctrl.GetHistory(null, null) as OkObjectResult;
        Assert.IsNotNull(res);

        var list = (res!.Value as IEnumerable<obligatorio.WebApi.DTO.PointsHistoryItemDto>)!.ToList();
        Assert.AreEqual(1, list.Count);

        var dto = list[0];
        Assert.AreEqual(at, dto.AtUtc);
        Assert.AreEqual(9, dto.Points);
        Assert.AreEqual("Access", dto.Origin);
        Assert.AreEqual(sid, dto.StrategyId);
        Assert.AreEqual("BaseStrategy", dto.StrategyName);
    }

    [TestMethod]
    public void GetHistory_UsesEmptyStrategyName_WhenMetaMissing()
    {
        var sid = Guid.NewGuid();
        var u1 = new User("Alice", "Baker", "alice@ex.com", "p1", new DateOnly(1990, 1, 1), MembershipLevel.Standard);

        var history = new Mock<IPointsHistoryLogic>();
        history.Setup(h => h.List(u1.Id, null, null))
            .Returns(new List<PointsAward>
            {
                new PointsAward
                {
                    Id = Guid.NewGuid(), User = u1, UserId = u1.Id, Points = 5, Reason = "Mission",
                    StrategyId = sid, At = DateTime.UtcNow
                }
            });

        var strategies = new Mock<IRepository<ScoringStrategyMeta>>();
        strategies.Setup(s => s.FindAll(It.IsAny<System.Linq.Expressions.Expression<Func<ScoringStrategyMeta, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<ScoringStrategyMeta, object>>[]>()))
            .Returns(new List<ScoringStrategyMeta>());

        var ctrl = new PointsController(history.Object, strategies.Object);
        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        ctrl.ControllerContext.HttpContext.Items["CurrentUserId"] = u1.Id;

        var res = ctrl.GetHistory(null, null) as OkObjectResult;
        Assert.IsNotNull(res);

        var list = (res!.Value as IEnumerable<obligatorio.WebApi.DTO.PointsHistoryItemDto>)!.ToList();
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual(string.Empty, list[0].StrategyName);
    }
}
