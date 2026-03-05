using System.IO;
using System.Threading.Tasks;
using Domain;

namespace IParkBusinessLogic;

public interface IPlugInLogic
{
    Task<ScoringStrategyMeta> UploadAsync(Stream dllStream, string originalFileName, string? displayName);
}
