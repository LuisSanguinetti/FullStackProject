using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class MaintenanceQueryLogic : IMaintenanceQueryLogic
{
    private readonly IRepository<Maintenance> _repo;

    public MaintenanceQueryLogic(IRepository<Maintenance> repo)
    {
        _repo = repo;
    }

    public bool IsAttractionUnderMaintenance(Guid attractionId, DateTime whenUtc)
    {
        var list = _repo.FindAll(m =>
            m.AttractionId == attractionId &&
            !m.Cancelled &&
            whenUtc >= m.StartAt &&
            whenUtc < m.StartAt.AddMinutes(m.DurationMinutes));

        return list.Count > 0;
    }

    public IReadOnlyList<Maintenance> List(Guid? attractionId = null, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        return _repo.FindAll(m =>
            (!attractionId.HasValue || m.AttractionId == attractionId.Value) &&
            (!fromUtc.HasValue || m.StartAt >= fromUtc.Value) &&
            (!toUtc.HasValue || m.StartAt < toUtc.Value))
            .OrderBy(m => m.StartAt)
            .ToList();
    }
}
