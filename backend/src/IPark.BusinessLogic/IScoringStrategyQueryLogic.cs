using Domain;

namespace IParkBusinessLogic;

public interface IScoringStrategyQueryLogic
{
    ScoringStrategyMeta GetActiveOrThrow();
}
