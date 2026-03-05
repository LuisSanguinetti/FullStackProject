using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class AwardPointsLogic : IAwardPointsLogic
{
    private readonly IScoringStrategyMetaLogic _meta;
    private readonly IRepository<PointsAward> _pointsAwardRepository;
    private readonly IAttractionHelperLogic _attractionHelperLogic;

    public AwardPointsLogic(IScoringStrategyMetaLogic meta, IRepository<PointsAward> pointsAwardRepository, IAttractionHelperLogic attractionHelperLogic)
    {
        _meta = meta;
        _pointsAwardRepository = pointsAwardRepository;
        _attractionHelperLogic = attractionHelperLogic;
    }

    public int ComputeForAccess(User user, AccessRecord access, DateTime at, Guid strategyId)
    {
        var strategy = ResolveActiveStrategy(strategyId);
        var points = strategy.PointsForAccess(user, access, at);
        CreatePointsAward(user, null, access, at, points, strategyId);
        return points;
    }

    public int ComputeForMission(User user, Mission mission, DateTime at, Guid strategyId)
    {
        var strategy = ResolveActiveStrategy(strategyId);
        var points = strategy.PointsForMission(user, mission, at);
        CreatePointsAward(user, mission, null, at, points, strategyId);
        return points;
    }

    private IScoringStrategy ResolveActiveStrategy(Guid strategyId)
    {
        var meta = _meta.GetActiveOrThrowById(strategyId);

        if (string.IsNullOrWhiteSpace(meta.FilePath))
        {
            throw new InvalidOperationException("Active strategy has no file path.");
        }

        var folder = Path.GetDirectoryName(meta.FilePath);
        if (string.IsNullOrWhiteSpace(folder))
        {
            throw new InvalidOperationException("Invalid plugin file path.");
        }

        var loader = new LoadAssembly<IScoringStrategy>(folder);
        var names = loader.GetImplementations();

        if (names.Count == 0)
        {
            throw new InvalidOperationException("Active plugin DLL does not contain an IScoringStrategy.");
        }

        if (names.Count > 1)
        {
            throw new InvalidOperationException("Active plugin DLL contains multiple IScoringStrategy implementations.");
        }

        var instance = loader.GetImplementation(0)
                       ?? throw new InvalidOperationException("Failed to instantiate IScoringStrategy from plugin.");

        return instance;
    }

    private void CreatePointsAward(User user, Mission? mission, AccessRecord? accessRecord, DateTime at, int points, Guid strategyId)
    {
        var description = string.Empty;
        if(accessRecord != null)
        {
            var attraction = _attractionHelperLogic.GetOrThrow(accessRecord.AttractionId);
            description = $"{points} awarded for going to attraction {attraction.Name}";
        }

        if(mission != null)
        {
            description = $"{points} awarded for completing mission {mission.Title}";
        }

        var pointsAward = new PointsAward(user.Id, user, points, description, strategyId, at);
        _pointsAwardRepository.Add(pointsAward);
    }
}
