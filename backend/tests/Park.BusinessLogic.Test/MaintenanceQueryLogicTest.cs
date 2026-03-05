using System;
using System.Collections.Generic;
using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Park.BusinessLogic;

namespace Park.BusinessLogic.Test;

[TestClass]
public class MaintenanceQueryLogicTests
{
    [TestMethod]
    public void IsAttractionUnderMaintenance_ReturnsTrue_WhenInsideWindow()
    {
        var attrId = Guid.NewGuid();
        var now = new DateTime(2025, 10, 10, 14, 0, 0, DateTimeKind.Utc);

        var maintenance = new Maintenance(attrId, new Attraction
        {
            Id = attrId,
            Name = "A",
            Type = AttractionType.RollerCoaster,
            MinAge = 0,
            MaxCapacity = 10,
            Description = "d",
            BasePoints = 1
        }, now.AddMinutes(-10), 30, "m");

        var repo = new Mock<IRepository<Maintenance>>();
        repo.Setup(r => r.FindAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Maintenance, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Maintenance, object>>[]>()))
            .Returns((System.Linq.Expressions.Expression<Func<Maintenance, bool>> p,
                System.Linq.Expressions.Expression<Func<Maintenance, object>>[] _) =>
            {
                var list = new List<Maintenance> { maintenance };
                var pred = p.Compile();
                return list.FindAll(m => pred(m));
            });

        var sut = new MaintenanceQueryLogic(repo.Object);

        var result = sut.IsAttractionUnderMaintenance(attrId, now);

        Assert.IsTrue(result);
    }

    [TestMethod]
    public void IsAttractionUnderMaintenance_ReturnsFalse_WhenCancelled()
    {
        var attrId = Guid.NewGuid();
        var start = new DateTime(2025, 10, 10, 14, 0, 0, DateTimeKind.Utc);
        var when = start.AddMinutes(5);

        var maint = new Maintenance(attrId, new Attraction
        {
            Id = attrId,
            Name = "A",
            Type = AttractionType.RollerCoaster,
            MinAge = 0,
            MaxCapacity = 10,
            Description = "d",
            BasePoints = 1
        }, start, 60, "m");
        maint.Cancel(start);

        var repo = new Mock<IRepository<Maintenance>>();
        repo.Setup(r => r.FindAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Maintenance, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Maintenance, object>>[]>()))
            .Returns((System.Linq.Expressions.Expression<Func<Maintenance, bool>> p,
                System.Linq.Expressions.Expression<Func<Maintenance, object>>[] _) =>
            {
                var list = new List<Maintenance> { maint };
                var pred = p.Compile();
                return list.FindAll(m => pred(m));
            });

        var sut = new MaintenanceQueryLogic(repo.Object);

        var result = sut.IsAttractionUnderMaintenance(attrId, when);

        Assert.IsFalse(result);
    }

    [TestMethod]
    public void List_FiltersByAttractionAndDates()
    {
        var a1 = Guid.NewGuid(); var a2 = Guid.NewGuid();
        var baseDt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var data = new List<Maintenance>
        {
            new Maintenance(a1, new Attraction{Id=a1,Name="A",Type=AttractionType.RollerCoaster,MinAge=0,MaxCapacity=1,Description="d",BasePoints=1}, baseDt.AddHours(1), 30, "x"),
            new Maintenance(a1, new Attraction{Id=a1,Name="A",Type=AttractionType.RollerCoaster,MinAge=0,MaxCapacity=1,Description="d",BasePoints=1}, baseDt.AddHours(5), 30, "y"),
            new Maintenance(a2, new Attraction{Id=a2,Name="B",Type=AttractionType.Show,MinAge=0,MaxCapacity=1,Description="d",BasePoints=1}, baseDt.AddHours(2), 30, "z"),
        };

        var repo = new Mock<IRepository<Maintenance>>();
        repo.Setup(r => r.FindAll(
                It.IsAny<System.Linq.Expressions.Expression<Func<Maintenance, bool>>>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Maintenance, object>>[]>()))
            .Returns((System.Linq.Expressions.Expression<Func<Maintenance, bool>> p,
                System.Linq.Expressions.Expression<Func<Maintenance, object>>[] _) =>
            {
                var pred = p.Compile();
                return data.FindAll(m => pred(m));
            });

        var sut = new MaintenanceQueryLogic(repo.Object);

        var result = sut.List(a1, baseDt, baseDt.AddHours(6));

        Assert.AreEqual(2, result.Count);
    }
}
