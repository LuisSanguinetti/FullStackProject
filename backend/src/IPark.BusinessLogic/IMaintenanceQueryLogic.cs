using Domain;

namespace IParkBusinessLogic;

public interface IMaintenanceQueryLogic
{
    bool IsAttractionUnderMaintenance(Guid attractionId, DateTime whenUtc);
    IReadOnlyList<Maintenance> List(Guid? attractionId = null, DateTime? fromUtc = null, DateTime? toUtc = null);
}
