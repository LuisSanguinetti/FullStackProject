using System;
using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class ScoringStrategyQueryLogic : IScoringStrategyQueryLogic
{
    private readonly IRepository<ScoringStrategyMeta> _repo;

    public ScoringStrategyQueryLogic(IRepository<ScoringStrategyMeta> repo)
    {
        _repo = repo;
    }

    public ScoringStrategyMeta GetActiveOrThrow()
    {
        var active = _repo.Find(s => s.IsActive && !s.IsDeleted);
        return active ?? throw new InvalidOperationException("No active scoring strategy configured");
    }
}
