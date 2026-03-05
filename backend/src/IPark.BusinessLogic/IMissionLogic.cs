using Domain;

namespace IParkBusinessLogic;

public interface IMissionLogic
{
    Mission CreateMission(string title, string description, int basePoints);
    Mission GetOrThrow(Guid missionId);
    public IList<Mission> ListPaged(int page, int pageSize);
}
