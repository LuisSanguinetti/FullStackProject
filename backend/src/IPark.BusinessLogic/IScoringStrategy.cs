using Domain;

namespace IParkBusinessLogic;

public interface IScoringStrategy
{
    int PointsForAccess(User user, AccessRecord accessRecord, DateTime at);
    int PointsForMission(User user, Mission mission, DateTime at);
}
