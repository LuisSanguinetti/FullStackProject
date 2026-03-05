using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class MissionLogic : IMissionLogic
{
    private readonly IRepository<Mission> _repoMission;

    public MissionLogic(IRepository<Mission> repoMission)
    {
        _repoMission = repoMission;
    }

    public Mission GetOrThrow(Guid id)
    {
        var m = _repoMission.Find(m => m.Id == id);
        if(m is null)
        {
            throw new InvalidOperationException("Mission not found");
        }

        return m;
    }

    public Mission CreateMission(string title, string description, int basePoints)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required.", nameof(description));
        }

        if (basePoints < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(basePoints), "BasePoints must be >= 0.");
        }

        var mission = new Mission(title.Trim(), description.Trim(), basePoints);
        _repoMission.Add(mission);
        return mission;
    }

    public IList<Mission> ListPaged(int page, int pageSize)
    {
        if (page < 1)
        {
            page = 1;
        }

        if (pageSize < 1 || pageSize > 100)
        {
            pageSize = 10;
        }

        return _repoMission.GetPage(
            page,
            pageSize,
            filter: null,
            orderBy: q => q.OrderBy(m => m.Title).ThenBy(m => m.Id));
    }
}
