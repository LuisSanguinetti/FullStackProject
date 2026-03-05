using System.Reflection;
using System.Reflection.Emit;
using Domain;
using IParkBusinessLogic;
using Moq;

namespace Park.BusinessLogic.Test;

[TestClass]
public class PlugInLogicTest
{
    private static string NewPluginsRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "plugins_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        return root;
    }

    private static MemoryStream Bytes(params byte[] data)
    {
        var ms = new MemoryStream(data);
        ms.Position = 0;
        return ms;
    }

    [TestMethod]
    public async Task UploadAsync_Throws_ArgumentNull_When_Stream_Is_Null()
    {
        var meta = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        var root = NewPluginsRoot();
        try
        {
            var logic = new PlugInLogic(meta.Object, root);

            await Assert.ThrowsExceptionAsync<ArgumentNullException>(
                () => logic.UploadAsync(null!, "x.dll", null));

            meta.VerifyNoOtherCalls();
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [TestMethod]
    public async Task UploadAsync_Throws_When_File_Is_Not_Dll()
    {
        var meta = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        var root = NewPluginsRoot();
        try
        {
            var logic = new PlugInLogic(meta.Object, root);

            using var stream = Bytes(0x01, 0x02);
            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => logic.UploadAsync(stream, "notdll.txt", "name"));
            StringAssert.Contains(ex.Message, "Only .dll files are allowed.");

            meta.VerifyNoOtherCalls();
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [TestMethod]
    public async Task UploadAsync_Throws_Invalid_For_BadImage_Dll_And_Cleans_Temp()
    {
        var meta = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        var root = NewPluginsRoot();
        try
        {
            var logic = new PlugInLogic(meta.Object, root);

            var beforeTmp = Directory.EnumerateDirectories(root, "tmp_*").ToArray();

            using var badDll = Bytes(0xDE, 0xAD, 0xBE, 0xEF);
            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => logic.UploadAsync(badDll, "bad.dll", "bad"));

            StringAssert.Contains(ex.Message, "Invalid or incompatible DLL", StringComparison.Ordinal);

            var afterTmp = Directory.EnumerateDirectories(root, "tmp_*").ToArray();
            CollectionAssert.AreEqual(beforeTmp, afterTmp);

            meta.VerifyNoOtherCalls();
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }

    [TestMethod]
    public async Task UploadAsync_SavesMeta_MovesFile_And_UpdatesPath()
    {
        // arrange
        var root = Path.Combine(Path.GetTempPath(), "plugins_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var metaId = Guid.NewGuid();
        var displayName = "no-strategy";
        var fileName = "no_strategy.dll";

        var metaLogic = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        metaLogic
            .Setup(m => m.CreateFromUploadAsync(displayName, It.IsAny<string>(), fileName))
            .ReturnsAsync(new ScoringStrategyMeta
            {
                Id = metaId,
                Name = displayName,
                FilePath = string.Empty,
                FileName = fileName,
                IsActive = false,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            });
        metaLogic
            .Setup(m => m.UpdatePathAsync(metaId, It.IsAny<string>(), fileName))
            .Returns(Task.CompletedTask);

        var logic = new PlugInLogic(metaLogic.Object, root);

        // use a valid managed assembly as bytes (this test assembly is fine)
        var asmPath = typeof(PlugInLogicTest).Assembly.Location;
        await using var ms = new MemoryStream(await File.ReadAllBytesAsync(asmPath));

        try
        {
            // act
            var result = await logic.UploadAsync(ms, fileName, displayName);

            // assert
            Assert.AreEqual(metaId, result.Id);

            var finalPath = Path.Combine(root, metaId.ToString("N"), fileName);
            Assert.IsTrue(File.Exists(finalPath), "Expected plugin file to be moved to final folder.");

            metaLogic.Verify(m => m.CreateFromUploadAsync(displayName, It.IsAny<string>(), fileName), Times.Once);
            metaLogic.Verify(m => m.UpdatePathAsync(metaId, It.IsAny<string>(), fileName), Times.Once);
            metaLogic.VerifyNoOtherCalls();
        }
        finally
        {
            try
            {
                Directory.Delete(root, true);
            }
            catch
            { /* best-effort cleanup */
            }
        }
    }

    [TestMethod]
    public async Task UploadAsync_Rethrows_And_DoesNot_UpdatePath_When_CreateFromUpload_Fails()
    {
        var root = Path.Combine(Path.GetTempPath(), "plugins_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var metaLogic = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        metaLogic
            .Setup(m => m.CreateFromUploadAsync(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var logic = new PlugInLogic(metaLogic.Object, root);

        var asmPath = typeof(PlugInLogicTest).Assembly.Location;
        await using var ms = new MemoryStream(await File.ReadAllBytesAsync(asmPath));

        try
        {
            var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                () => logic.UploadAsync(ms, "x.dll", "disp"));
            StringAssert.Contains(ex.Message, "boom");

            metaLogic.Verify(m => m.CreateFromUploadAsync(It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            metaLogic.Verify(m => m.UpdatePathAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { /* best-effort cleanup */ }
        }
    }

    [TestMethod]
    public async Task UploadAsync_Deletes_Temp_On_Success()
    {
        var root = Path.Combine(Path.GetTempPath(), "plugins_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var metaId = Guid.NewGuid();
        const string fileName = "ok.dll";
        const string display = "disp";

        var metaLogic = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        metaLogic
            .Setup(m => m.CreateFromUploadAsync(display, It.IsAny<string>(), fileName))
            .ReturnsAsync(new ScoringStrategyMeta
            {
                Id = metaId,
                Name = display,
                FilePath = string.Empty,
                FileName = fileName,
                IsActive = false,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            });
        metaLogic
            .Setup(m => m.UpdatePathAsync(metaId, It.IsAny<string>(), fileName))
            .Returns(Task.CompletedTask);

        var logic = new PlugInLogic(metaLogic.Object, root);

        var asmPath = typeof(PlugInLogicTest).Assembly.Location;
        await using var ms = new MemoryStream(await File.ReadAllBytesAsync(asmPath));

        try
        {
            var result = await logic.UploadAsync(ms, fileName, display);
            Assert.AreEqual(metaId, result.Id);

            var finalPath = Path.Combine(root, metaId.ToString("N"), fileName);
            Assert.IsTrue(File.Exists(finalPath), "Final plugin file should exist.");

            var leftovers = Directory.EnumerateDirectories(root, "tmp_*")
                .SelectMany(d => Directory.EnumerateFiles(d, fileName, SearchOption.TopDirectoryOnly))
                .ToArray();
            Assert.AreEqual(0, leftovers.Length, "Temp copy of the DLL should be removed after success.");
        }
        finally
        {
            try
            {
                Directory.Delete(root, true);
            }
            catch
            { /* best-effort cleanup */
            }
        }
    }

    [TestMethod]
    public void TryDeleteFolder_NoOp_When_Folder_Missing()
    {
        var missing = Path.Combine(Path.GetTempPath(), "plugins_test_missing_" + Guid.NewGuid().ToString("N"));
        var mi = typeof(PlugInLogic).GetMethod("TryDeleteFolder", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.IsNotNull(mi);
        mi.Invoke(null, new object?[] { missing });
    }

    [TestMethod]
    public void TryCopyContractAssemblyTo_Copies_Once_And_Is_Idempotent()
    {
        // arrange
        var target = Path.Combine(Path.GetTempPath(), "plugins_copy_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(target);

        try
        {
            var mi = typeof(PlugInLogic).GetMethod(
                "TryCopyContractAssemblyTo",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(mi);

            var contractAsm = typeof(IParkBusinessLogic.IScoringStrategy).Assembly;
            var dllName = Path.GetFileName(contractAsm.Location);
            var dllDest = Path.Combine(target, dllName);

            // act:
            mi.Invoke(null, new object?[] { target, contractAsm });
            var existsAfterFirst = File.Exists(dllDest);

            // act:
            mi.Invoke(null, new object?[] { target, contractAsm });
            var existsAfterSecond = File.Exists(dllDest);

            // assert
            Assert.IsTrue(existsAfterFirst, "Expected contract DLL to be copied on first call.");
            Assert.IsTrue(existsAfterSecond, "Contract DLL should still exist after idempotent second call.");
        }
        finally
        {
            try { Directory.Delete(target, true); } catch { /* best-effort */ }
        }
    }

    [TestMethod]
    public void TryCopyContractAssemblyTo_EarlyReturn_When_Assembly_Has_No_Location()
    {
        // arrange:
        var target = Path.Combine(Path.GetTempPath(), "plugins_copy_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(target);

        try
        {
            var mi = typeof(PlugInLogic).GetMethod(
                "TryCopyContractAssemblyTo",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(mi);

            var asmName = new AssemblyName("NoLocationAsm");
            var dyn = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);

            // act
            mi.Invoke(null, new object?[] { target, dyn });

            // assert:
            Assert.AreEqual(0, Directory.EnumerateFiles(target).Count());
        }
        finally
        {
            try { Directory.Delete(target, true); } catch { /* best-effort */ }
        }
    }

    [TestMethod]
    public async Task UploadAsync_Uses_TestPlugin_And_Writes_To_Final_Folder()
    {
        // arrange
        var repoRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        var pluginPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "TestPlugIn", "Plugins.dll"));

        var root = Path.Combine(Path.GetTempPath(), "plugins_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var metaId = Guid.NewGuid();
        const string display = "EventBonusStrategy";
        const string fileName = "Plugins.dll";

        var metaLogic = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        metaLogic
            .Setup(m => m.CreateFromUploadAsync(display, It.IsAny<string>(), fileName))
            .ReturnsAsync(new ScoringStrategyMeta
            {
                Id = metaId,
                Name = display,
                FilePath = string.Empty,
                FileName = fileName,
                IsActive = false,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            });
        metaLogic
            .Setup(m => m.UpdatePathAsync(metaId, It.IsAny<string>(), fileName))
            .Returns(Task.CompletedTask);

        var logic = new PlugInLogic(metaLogic.Object, root);

        await using var ms = new MemoryStream(await File.ReadAllBytesAsync(pluginPath));

        try
        {
            // act
            var meta = await logic.UploadAsync(ms, fileName, display);

            // assert
            Assert.AreEqual(metaId, meta.Id);
            var finalPath = Path.Combine(root, metaId.ToString("N"), fileName);
            Assert.IsTrue(File.Exists(finalPath));

            metaLogic.Verify(m => m.CreateFromUploadAsync(display, It.IsAny<string>(), fileName), Times.Once);
            metaLogic.Verify(m => m.UpdatePathAsync(metaId, It.IsAny<string>(), fileName), Times.Once);
            metaLogic.VerifyNoOtherCalls();
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { /* best-effort */ }
        }
    }

    [TestMethod]
    public async Task UploadAsync_Throws_ArgumentException_When_FileName_Is_Empty()
    {
        var meta = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        var root = Path.Combine(Path.GetTempPath(), "plugins_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var logic = new PlugInLogic(meta.Object, root);
        await using var ms = new MemoryStream(new byte[] { 0x01 });

        try
        {
            await Assert.ThrowsExceptionAsync<ArgumentException>(() =>
                logic.UploadAsync(ms, string.Empty, "disp"));
            meta.VerifyNoOtherCalls();
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }
    }

    [TestMethod]
    public async Task UploadAsync_Uses_Default_PluginsRoot_When_Constructor_Receives_Empty_Root()
    {
        // arrange:
        var dllPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "TestPlugIn", "Plugins.dll"));
        Assert.IsTrue(File.Exists(dllPath), $"Plugins.dll not found at: {dllPath}");

        var metaId = Guid.NewGuid();
        const string fileName = "Plugins.dll";
        const string display = "disp";

        var metaLogic = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        metaLogic
            .Setup(m => m.CreateFromUploadAsync(display, It.IsAny<string>(), fileName))
            .ReturnsAsync(new ScoringStrategyMeta
            {
                Id = metaId,
                Name = display,
                FilePath = string.Empty,
                FileName = fileName,
                IsActive = false,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            });
        metaLogic
            .Setup(m => m.UpdatePathAsync(metaId, It.IsAny<string>(), fileName))
            .Returns(Task.CompletedTask);

        var logic = new PlugInLogic(metaLogic.Object, pluginsRoot: string.Empty);

        await using var ms = new MemoryStream(await File.ReadAllBytesAsync(dllPath));

        // act
        var meta = await logic.UploadAsync(ms, fileName, display);

        // assert:
        var expectedRoot = Path.Combine(AppContext.BaseDirectory, "Plugins");
        var finalPath = Path.Combine(expectedRoot, metaId.ToString("N"), fileName);
        Assert.AreEqual(metaId, meta.Id);
        Assert.IsTrue(File.Exists(finalPath));

        // cleanup
        try { Directory.Delete(expectedRoot, true); } catch { }

        metaLogic.Verify(m => m.CreateFromUploadAsync(display, It.IsAny<string>(), fileName), Times.Once);
        metaLogic.Verify(m => m.UpdatePathAsync(metaId, It.IsAny<string>(), fileName), Times.Once);
        metaLogic.VerifyNoOtherCalls();
    }

    [TestMethod]
    public void TryCopyContractAssemblyTo_Swallows_Copy_Errors_When_Target_Is_ReadOnly()
    {
        var target = Path.Combine(Path.GetTempPath(), "plugins_copy_ro_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(target);

        var di = new DirectoryInfo(target) { Attributes = FileAttributes.ReadOnly };

        try
        {
            var mi = typeof(PlugInLogic).GetMethod("TryCopyContractAssemblyTo",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.IsNotNull(mi);

            var contractAsm = typeof(IParkBusinessLogic.IScoringStrategy).Assembly;

            // act:
            mi.Invoke(null, new object?[] { target, contractAsm });

            // assert:
            Assert.IsTrue(Directory.Exists(target));
        }
        finally
        {
            new DirectoryInfo(target).Attributes = FileAttributes.Normal;
            try { Directory.Delete(target, true); } catch { }
        }
    }

    [TestMethod]
    public async Task UploadAsync_Overwrites_Final_File_If_It_Already_Exists()
    {
        // arrange
        var dllPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "TestPlugIn", "Plugins.dll"));
        Assert.IsTrue(File.Exists(dllPath), $"Plugins.dll not found at: {dllPath}");

        var root = Path.Combine(Path.GetTempPath(), "plugins_test_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);

        var metaId = Guid.NewGuid();
        const string fileName = "Plugins.dll";
        const string display = "disp";

        var metaLogic = new Mock<IScoringStrategyMetaLogic>(MockBehavior.Strict);
        metaLogic
            .Setup(m => m.CreateFromUploadAsync(display, It.IsAny<string>(), fileName))
            .ReturnsAsync(new ScoringStrategyMeta
            {
                Id = metaId,
                Name = display,
                FilePath = string.Empty,
                FileName = fileName,
                IsActive = false,
                IsDeleted = false,
                CreatedOn = DateTime.UtcNow
            });
        metaLogic
            .Setup(m => m.UpdatePathAsync(metaId, It.IsAny<string>(), fileName))
            .Returns(Task.CompletedTask);

        var logic = new PlugInLogic(metaLogic.Object, root);

        var finalFolder = Path.Combine(root, metaId.ToString("N"));
        Directory.CreateDirectory(finalFolder);
        var finalPath = Path.Combine(finalFolder, fileName);
        await File.WriteAllBytesAsync(finalPath, new byte[] { 0xAA, 0xBB, 0xCC });

        await using var ms = new MemoryStream(await File.ReadAllBytesAsync(dllPath));

        try
        {
            // act
            var meta = await logic.UploadAsync(ms, fileName, display);

            // assert:
            var newLen = new FileInfo(finalPath).Length;
            var origLen = new FileInfo(dllPath).Length;
            Assert.AreEqual(metaId, meta.Id);
            Assert.AreEqual(origLen, newLen);

            metaLogic.Verify(m => m.CreateFromUploadAsync(display, It.IsAny<string>(), fileName), Times.Once);
            metaLogic.Verify(m => m.UpdatePathAsync(metaId, It.IsAny<string>(), fileName), Times.Once);
            metaLogic.VerifyNoOtherCalls();
        }
        finally
        {
            try { Directory.Delete(root, true); } catch { }
        }
    }
}
