using Domain;

namespace IParkBusinessLogic;

public interface IScoringStrategyMetaLogic
{
    Task ActivateAsync(Guid id);
    Task SoftDeleteAsync(Guid id);
    Task<ScoringStrategyMeta> CreateFromUploadAsync(string? displayName, string filePath, string fileName);
    List<ScoringStrategyMeta> GetActiveOrThrow();
    Task UpdatePathAsync(Guid id, string filePath, string? fileName = null);
    Task<List<ScoringStrategyMeta>> ListAsync(bool includeDeleted = false);
    ScoringStrategyMeta GetActiveOrThrowById(Guid strategyId);
}
