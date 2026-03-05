namespace IParkBusinessLogic;

public interface IMissionCompletionLogic
{
    int Register(Guid userId, Guid missionId, DateTime completedAtUtc, Guid scoringStrategyId);
}
