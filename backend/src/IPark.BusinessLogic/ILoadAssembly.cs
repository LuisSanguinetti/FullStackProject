namespace IParkBusinessLogic;

public interface ILoadAssembly<IInterface> where IInterface : class
{
    List<string> GetImplementations();
    IInterface? GetImplementation(int index, params object[] args);
}
