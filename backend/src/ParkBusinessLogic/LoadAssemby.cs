using System.Reflection;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public sealed class LoadAssembly<IInterface> : ILoadAssembly<IInterface> where IInterface : class
{
    private readonly DirectoryInfo directory;
    private readonly List<(Type Type, string FilePath, string FileName)> implementations = new();

    public LoadAssembly(string path)
    {
        if (File.Exists(path))
        {
            directory = new DirectoryInfo(Path.GetDirectoryName(path)!);
        }
        else
        {
            directory = new DirectoryInfo(path);
        }
    }

    public List<string> GetImplementations()
    {
        implementations.Clear();

        if (!directory.Exists)
        {
            return new List<string>();
        }

        var files = directory.GetFiles("*.dll").ToList();

        foreach (var file in files)
        {
            var asm = Assembly.LoadFile(file.FullName);

            var loadedTypes = asm
                .GetTypes()
                .Where(t =>
                    t.IsClass &&
                    !t.IsAbstract &&
                    typeof(IInterface).IsAssignableFrom(t) &&
                    t.GetConstructor(Type.EmptyTypes) is not null)
                .ToList();

            if (loadedTypes.Count == 0)
            {
                Console.WriteLine($"No '{typeof(IInterface).Name}' implementation in: {file.FullName}");
                continue;
            }

            foreach (var t in loadedTypes)
            {
                if (implementations.Any(x => x.Type.FullName == t.FullName))
                {
                    continue;
                }

                implementations.Add((t, file.FullName, file.Name));
            }
        }

        return implementations
            .Select(x => $"{x.FileName}: {x.Type.Name}")
            .ToList();
    }

    public IInterface? GetImplementation(int index, params object[] args)
    {
        if (index < 0 || index >= implementations.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Invalid implementation index.");
        }

        var (type, _, _) = implementations[index];
        return Activator.CreateInstance(type, args) as IInterface;
    }
}
