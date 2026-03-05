using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class CustomDateTimeProviderTest
{
    [TestMethod]
    public void GetNowUtc_Default_Uses_System_UtcNow()
    {
        var provider = new CustomDateTimeProvider();

        var before = DateTime.UtcNow;
        var now = provider.GetNowUtc();
        var after = DateTime.UtcNow;

        Assert.IsTrue(now >= before && now <= after,
            $"Expected {now:o} to be between {before:o} and {after:o}");
    }

    [TestMethod]
    public void SetCustomTime_Makes_GetNowUtc_Return_Custom()
    {
        var provider = new CustomDateTimeProvider();
        var custom = new DateTime(2025, 10, 28, 21, 00, 00, DateTimeKind.Utc);

        provider.SetCustomTime(custom);

        var now = provider.GetNowUtc();
        Assert.AreEqual(custom, now);
    }

    [TestMethod]
    public void ClearCustomTime_Reverts_To_System_UtcNow()
    {
        var provider = new CustomDateTimeProvider();
        var custom = new DateTime(2001, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        provider.SetCustomTime(custom);

        provider.ClearCustomTime();

        var before = DateTime.UtcNow;
        var now = provider.GetNowUtc();
        var after = DateTime.UtcNow;

        Assert.IsTrue(now >= before && now <= after,
            $"Expected {now:o} to be between {before:o} and {after:o}");
    }

    [TestMethod]
    public void SetCustomTime_Overwrites_Previous_Value()
    {
        var provider = new CustomDateTimeProvider();
        var first = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var second = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        provider.SetCustomTime(first);
        provider.SetCustomTime(second);

        Assert.AreEqual(second, provider.GetNowUtc());
    }
}
