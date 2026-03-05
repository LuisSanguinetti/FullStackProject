using Domain;

namespace IParkBusinessLogic;

public interface IPointsHistoryLogic
{
    IReadOnlyList<PointsAward> List(Guid userId, DateTime? fromUtc = null, DateTime? toUtc = null);
}
