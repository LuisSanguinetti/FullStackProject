using Domain;
using System.Reflection;
using FluentAssertions;
using IParkBusinessLogic;

namespace Park.BusinessLogic.Test;

[TestClass]
public class LoadAssemlyTest
{
    private sealed class TestStrategy : IScoringStrategy
    {
        public int PointsForAccess(Domain.User user, Domain.AccessRecord accessRecord, DateTime at) => 7;
        public int PointsForMission(Domain.User user, Domain.Mission mission, DateTime at) => 11;
    }

    private static string CreateTempPluginsDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), "plugins_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void CopyAs(string sourcePath, string destDir, string destFileName)
    {
        var destPath = Path.Combine(destDir, destFileName);
        File.Copy(sourcePath, destPath, overwrite: true);
    }

    [TestMethod]
    public void GetImplementations_ReturnsEmpty_WhenDirectoryMissing()
    {
        // arrange
        var missing = Path.Combine(Path.GetTempPath(), "missing_" + Guid.NewGuid().ToString("N"));
        var loader = new LoadAssembly<IScoringStrategy>(missing);

        // act
        var names = loader.GetImplementations();

        // assert
        names.Should().NotBeNull().And.BeEmpty();
    }

    [TestMethod]
    public void GetImplementations_ListsOnlyDllsWithImplementations_AndSkipsThoseWithout()
    {
        // arrange: make a temp folder with two dlls:
        var plugins = CreateTempPluginsDir();

        var thisAssembly = Assembly.GetExecutingAssembly().Location;
        CopyAs(thisAssembly, plugins, "Plugin.WithStrategy.dll");

        var noImplSource = typeof(Enumerable).Assembly.Location;
        CopyAs(noImplSource, plugins, "Plugin.NoStrategy.dll");

        var loader = new LoadAssembly<IScoringStrategy>(plugins);

        // act
        var names = loader.GetImplementations();

        // assert:
        names.Should().NotBeNull();
        names.Should().ContainSingle(n => n.StartsWith("Plugin.WithStrategy.dll", StringComparison.OrdinalIgnoreCase));
        names.Should().NotContain(n => n.StartsWith("Plugin.NoStrategy.dll", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void GetImplementation_CreatesInstance_AndOutOfRangeThrows()
    {
        // arrange
        var plugins = CreateTempPluginsDir();
        var thisAssembly = Assembly.GetExecutingAssembly().Location;
        CopyAs(thisAssembly, plugins, "Plugin.WithStrategy.dll");

        var loader = new LoadAssembly<IScoringStrategy>(plugins);
        var names = loader.GetImplementations().ToList();

        // pick the first entry from the copied dll (there may be >1 strategies now)
        var idx = names.FindIndex(n => n.StartsWith("Plugin.WithStrategy.dll", StringComparison.OrdinalIgnoreCase));
        Assert.IsTrue(idx >= 0, "Expected at least one strategy in Plugin.WithStrategy.dll");

        // act
        var instance = loader.GetImplementation(idx);

        // assert:
        Assert.IsNotNull(instance);
        var access = instance!.PointsForAccess(null!, null!, DateTime.UtcNow);
        var mission = instance!.PointsForMission(null!, null!, DateTime.UtcNow);
        CollectionAssert.Contains(new[] { 5, 7 }, access, "PointsForAccess should match one of known test strategies");
        CollectionAssert.Contains(new[] { 9, 11 }, mission, "PointsForMission should match one of known test strategies");

        // out-of-range still throws
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => loader.GetImplementation(names.Count));
    }
}
