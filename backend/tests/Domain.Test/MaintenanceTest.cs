using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Domain.Test;

[TestClass]
public class MaintenanceTests
{
    private static Attraction Attr() => new Attraction
    {
        Id = Guid.NewGuid(),
        Name = "A",
        Type = AttractionType.RollerCoaster,
        MinAge = 0,
        MaxCapacity = 10,
        Description = "d",
        BasePoints = 1
    };

    [TestMethod]
    public void IsActiveAt_True_InsideWindow()
    {
        var start = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var m = new Maintenance(Attr().Id, Attr(), start, 30, "desc");
        Assert.IsTrue(m.IsActiveAt(start.AddMinutes(10)));
    }

    [TestMethod]
    public void IsActiveAt_False_OutsideWindow()
    {
        var start = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var m = new Maintenance(Attr().Id, Attr(), start, 30, "desc");
        Assert.IsFalse(m.IsActiveAt(start.AddMinutes(40)));
    }

    [TestMethod]
    public void Cancel_BeforeEnd_MarksCancelled()
    {
        var start = DateTime.UtcNow.AddHours(1);
        var m = new Maintenance(Attr().Id, Attr(), start, 60, "desc");
        m.Cancel(start.AddMinutes(10));
        Assert.IsTrue(m.Cancelled);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void Cancel_AfterEnd_Throws()
    {
        var start = DateTime.UtcNow;
        var m = new Maintenance(Attr().Id, Attr(), start, 10, "desc");
        m.Cancel(start.AddMinutes(30));
    }
}
