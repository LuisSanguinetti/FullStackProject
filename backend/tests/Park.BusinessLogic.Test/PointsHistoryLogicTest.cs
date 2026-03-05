using Domain;
using IDataAccess;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Park.BusinessLogic;
using System;
using System.Collections.Generic;
using System.Linq;

[TestClass]
public class PointsHistoryLogicTest
{
    [TestMethod]
    public void List_FiltersByUser_AndDate_AndOrdersDesc()
    {
        var userId = Guid.NewGuid();
        var s1 = Guid.NewGuid();
        var u = new User("Bob", "Carson", "1@x", "p", new DateOnly(1990, 1, 1), MembershipLevel.Standard) { Id = userId };

        var data = new List<PointsAward>
        {
            new PointsAward { User = u,Id=Guid.NewGuid(), UserId=userId, Points=10, Reason="Atracción", StrategyId=s1, At=new DateTime(2025,1,11,10,0,0,DateTimeKind.Utc)},
            new PointsAward { User = u,Id=Guid.NewGuid(), UserId=userId, Points=5,  Reason="Misión",    StrategyId=s1, At=new DateTime(2025,1,10,10,0,0,DateTimeKind.Utc)},
            new PointsAward { User = u,Id=userId, UserId=Guid.NewGuid(), Points=999, Reason="Otro", StrategyId=s1, At=new DateTime(2025,1,12,10,0,0,DateTimeKind.Utc)}
        };

        var repo = new Mock<IRepository<PointsAward>>();
        repo.Setup(r => r.FindAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<PointsAward, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<PointsAward, object>>[]>()))
            .Returns((System.Linq.Expressions.Expression<Func<PointsAward, bool>> p,
                System.Linq.Expressions.Expression<Func<PointsAward, object>>[] _) =>
            {
                var pred = p.Compile();
                return data.Where(pred).ToList();
            });

        var sut = new PointsHistoryLogic(repo.Object);

        var from = new DateTime(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2025, 1, 12, 0, 0, 0, DateTimeKind.Utc);

        var list = sut.List(userId, from, to);

        Assert.AreEqual(2, list.Count);
        Assert.IsTrue(list[0].At > list[1].At);
        Assert.AreEqual("Atracción", list[0].Reason);
    }
}
