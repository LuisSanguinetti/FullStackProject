using Domain;

namespace IParkBusinessLogic;

public interface IAwardPointsLogic
{
    int ComputeForAccess(User user, AccessRecord access, DateTime at, Guid strategyId);
    int ComputeForMission(User user, Mission mission, DateTime at, Guid strategyId);
}
