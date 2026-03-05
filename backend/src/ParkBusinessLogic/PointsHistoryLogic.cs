using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class PointsHistoryLogic : IPointsHistoryLogic
{
    private readonly IRepository<PointsAward> _awards;

    public PointsHistoryLogic(IRepository<PointsAward> awards)
    {
        _awards = awards;
    }

    public IReadOnlyList<PointsAward> List(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null)
    {
        return _awards.FindAll(pa =>
                pa.UserId == userId &&
                (!fromUtc.HasValue || pa.At >= fromUtc.Value) &&
                (!toUtc.HasValue || pa.At < toUtc.Value))
            .OrderByDescending(pa => pa.At)
            .ToList();
    }
}
