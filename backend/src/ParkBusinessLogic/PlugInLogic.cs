using System.Reflection;
using Domain;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public sealed class PlugInLogic : IPlugInLogic
{
    private readonly IScoringStrategyMetaLogic _metaLogic;
    private readonly string _pluginsRoot;

    public PlugInLogic(IScoringStrategyMetaLogic metaLogic, string pluginsRoot)
    {
        _metaLogic = metaLogic ?? throw new ArgumentNullException(nameof(metaLogic));

        _pluginsRoot = string.IsNullOrWhiteSpace(pluginsRoot)
            ? Path.Combine(AppContext.BaseDirectory, "Plugins")
            : Path.GetFullPath(pluginsRoot);

        Directory.CreateDirectory(_pluginsRoot);
    }

public async Task<ScoringStrategyMeta> UploadAsync(Stream dllStream, string originalFileName, string? displayName)
{
    ArgumentNullException.ThrowIfNull(dllStream);
    if (string.IsNullOrWhiteSpace(originalFileName))
    {
        throw new ArgumentException("Invalid file name.", nameof(originalFileName));
    }

    var ext = Path.GetExtension(originalFileName);
    if (!string.Equals(ext, ".dll", StringComparison.OrdinalIgnoreCase))
    {
        throw new InvalidOperationException("Only .dll files are allowed.");
    }

    var tempFolder = Path.Combine(_pluginsRoot, "tmp_" + Guid.NewGuid().ToString("N"));
    Directory.CreateDirectory(tempFolder);

    var safeName = Path.GetFileName(originalFileName);
    var uploadedPath = Path.Combine(tempFolder, safeName);

    await using (var fs = File.Create(uploadedPath))
    {
        await dllStream.CopyToAsync(fs);
    }

    TryCopyContractAssemblyTo(tempFolder, typeof(IScoringStrategy).Assembly);

    try
    {
        // Load and validate plugin types
        var loader = new LoadAssembly<IScoringStrategy>(tempFolder);

        // Works for List<string> or List<Type> (or IEnumerable<...>)
        var implementations = loader.GetImplementations();

        // Save meta first with temp path, then move to final folder
        var meta = await _metaLogic.CreateFromUploadAsync(displayName, uploadedPath, safeName);

        var finalFolder = Path.Combine(_pluginsRoot, meta.Id.ToString("N"));
        Directory.CreateDirectory(finalFolder);

        var finalPath = Path.Combine(finalFolder, safeName);
        File.Move(uploadedPath, finalPath, overwrite: true);

        TryDeleteFolder(tempFolder);

        await _metaLogic.UpdatePathAsync(meta.Id, finalPath, safeName);
        return meta;
    }
    catch (ReflectionTypeLoadException rtle)
    {
        // Aggregate loader exception messages for clarity
        var msgs = rtle.LoaderExceptions?
            .Where(e => e is not null)
            .Select(e => e!.Message)
            .Distinct()
            .ToArray() ?? Array.Empty<string>();

        var details = msgs.Length > 0 ? string.Join(" | ", msgs) : "Unknown type loading error.";
        TryDeleteFolder(tempFolder);
        throw new InvalidOperationException($"Failed to load plugin types. Dependency/ABI issue: {details}");
    }
    catch (BadImageFormatException bif)
    {
        TryDeleteFolder(tempFolder);
        throw new InvalidOperationException($"Invalid or incompatible DLL: {bif.Message}");
    }
    catch
    {
        TryDeleteFolder(tempFolder);
        throw;
    }
}

private static void TryCopyContractAssemblyTo(string targetFolder, Assembly contractAsm)
{
    try
    {
        var asmPath = contractAsm.Location;
        if (string.IsNullOrWhiteSpace(asmPath) || !File.Exists(asmPath))
        {
            return;
        }

        var dest = Path.Combine(targetFolder, Path.GetFileName(asmPath));
        if (!File.Exists(dest))
        {
            File.Copy(asmPath, dest, overwrite: false);
        }

        var pdbSrc = Path.ChangeExtension(asmPath, ".pdb");
        if (File.Exists(pdbSrc))
        {
            var pdbDest = Path.Combine(targetFolder, Path.GetFileName(pdbSrc));
            if (!File.Exists(pdbDest))
            {
                File.Copy(pdbSrc, pdbDest, overwrite: false);
            }
        }
    }
    catch
    {
        // Best-effort only.
    }
}

    private static void TryDeleteFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        try
        {
            Directory.Delete(folder, recursive: true);
        }
        catch (IOException)
        { /* file locked during load; ignore */
        }
        catch (UnauthorizedAccessException)
        { /* Windows lock; ignore */
        }
    }
}
