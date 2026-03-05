using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class MissionCompletionLogic : IMissionCompletionLogic
{
    private readonly IMissionLogic _missions;
    private readonly IUserLogic _users;
    private readonly IAwardPointsLogic _awards;
    private readonly IRepository<MissionCompletion> _repo;

    public MissionCompletionLogic(
        IMissionLogic missions,
        IUserLogic users,
        IAwardPointsLogic awards,
        IRepository<MissionCompletion> repo)
    {
        _missions = missions;
        _users = users;
        _awards = awards;
        _repo = repo;
    }

    public int Register(Guid userId, Guid missionId, DateTime completedAtUtc, Guid scoringStrategyId)
    {
        var user = _users.GetOrThrow(userId);
        var mission = _missions.GetOrThrow(missionId);

        var existing = _repo.Find(mc => mc.UserId == user.Id && mc.MissionId == mission.Id);
        if (existing is not null)
        {
            return existing.Points;
        }

        var points = _awards.ComputeForMission(user, mission, completedAtUtc, scoringStrategyId);

        var completion = new MissionCompletion(
            userId: user.Id,
            missionId: mission.Id,
            at: completedAtUtc,
            points: points);

        _repo.Add(completion);
        _users.AddPoints(user.Id, points);

        return points;
    }
}
