using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic;

public class AccessRecordLogic : IAccessRecordLogic
{
    private readonly IRepository<AccessRecord> _repo;

    private readonly ITicketLogic _ticketLogic;
    private readonly IAttractionHelperLogic _attractions;
    private readonly IAwardPointsLogic _awards;
    private readonly IUserLogic _users;
    private readonly IIncidentLogic _incidentLogic;
    private readonly ISpecialEventLogic _specialEventLogic;
    private readonly IMaintenanceQueryLogic _maintenanceLogic;

    public AccessRecordLogic(
        IRepository<AccessRecord> repo,
        ITicketLogic ticketLogic,
        IAttractionHelperLogic attractions,
        IAwardPointsLogic awards,
        IUserLogic users,
        IIncidentLogic incidentLogic,
        ISpecialEventLogic specialEventLogic,
        IMaintenanceQueryLogic maintenanceLogic
    )
    {
        _repo = repo;
        _ticketLogic = ticketLogic;
        _attractions = attractions;
        _awards = awards;
        _users = users;
        _incidentLogic = incidentLogic;
        _specialEventLogic = specialEventLogic;
        _maintenanceLogic = maintenanceLogic;
    }

    public int? Register(Guid ticketQr, Guid attractionId, DateTime nowUtc, Guid scoringStrategyId)
    {
        var ticket = _ticketLogic.GetByQrOrThrow(ticketQr);
        var attraction = _attractions.GetOrThrow(attractionId);

        if(_maintenanceLogic.IsAttractionUnderMaintenance(attraction.Id, nowUtc))
        {
            throw new InvalidOperationException($"La atraccion '{attraction.Name}' esta en mantenimiento y no admite ingresos.");
        }

        if(ticket.Owner is null)
        {
            throw new KeyNotFoundException($"Ticket owner {ticket.UserId} not found.");
        }

        var user = _users.GetOrThrow(ticket.UserId);

        if(_users.CalculateAge(user.Id) <= attraction.MinAge)
        {
            throw new AgeRequirementNotMetException(user.Name);
        }

        if(ValidateMaximumCapacity(attraction.Id))
        {
            throw new AttractionCapacityReachedException(attraction.Name, attraction.MaxCapacity);
        }

        if(!attraction.Enabled)
        {
            throw new AttractionDisabledException(attraction.Name);
        }

        if(_specialEventLogic.IsAttractionReferenced(attractionId))
        {
            if(ValidateSpecialEvent(ticket.QrCode, attraction.Id, _specialEventLogic.GetEventIdsByAttraction(attractionId)))
            {
                throw new SpecialEventAttractionMismatchException(attraction.Name, ticket.SpecialEvent.Name);
            }
        }

        var access = new AccessRecord
        {
            Id = Guid.NewGuid(),
            InAt = nowUtc,
            OutAt = null,
            TicketId = ticket.Id,
            Ticket = ticket,
            AttractionId = attraction.Id,
            Attraction = attraction,
            SpecialEventId = ticket.SpecialEventId,
            Points = 0
        };

        var points = _awards.ComputeForAccess(ticket.Owner, access, nowUtc, scoringStrategyId);
        access.Points = points;
        _users.AddPoints(ticket.UserId, points);
        _repo.Add(access);

        return access.Points;
    }

    public IList<AccessRecord> FindByAttractionAndDate(Guid attractionId, DateTime startDate, DateTime endDate)
    {
        return _repo.FindAll(ar =>
            ar.AttractionId == attractionId &&
            ar.InAt >= startDate &&
            ar.InAt <= endDate);
    }

    public bool ValidateAge(Guid ticketQr, Guid attractionId)
    {
        Attraction attraction = _attractions.GetOrThrow(attractionId);
        Ticket ticket = _ticketLogic.GetByQrOrThrow(ticketQr);
        var age = _users.CalculateAge(ticket.Owner.Id);

        return age >= attraction.MinAge;
    }

    public int CheckCurrentCapacity(Guid attractionId)
    {
        return _repo.FindAll(a => a.AttractionId == attractionId && a.OutAt == null).Count;
    }

    public bool ValidateMaximumCapacity(Guid attractionId)
    {
        var capacity = CheckCurrentCapacity(attractionId);
        var attraction = _attractions.GetOrThrow(attractionId);
        return capacity >= attraction.MaxCapacity;
    }

    public void RegisterExit(Guid accessRecordId, DateTime nowUtc)
    {
        var access = _repo.Find(ar => ar.Id == accessRecordId)
                ?? throw new KeyNotFoundException($"Access record {accessRecordId} not found.");

        access.OutAt = nowUtc;
        _repo.Update(access);
    }

    public int RemainingPeopleCapacity(Guid attractionId)
    {
        return _attractions.GetOrThrow(attractionId).MaxCapacity - CheckCurrentCapacity(attractionId);
    }

    public bool ValidateSpecialEvent(Guid ticketQr, Guid attractionId, IEnumerable<Guid> specialEventIds)
    {
        var ticket = _ticketLogic.GetByQrOrThrow(ticketQr);

        if(ticket.Type != TicketType.SpecialEvent)
        {
            return true;
        }

        if(ticket.SpecialEventId == null)
        {
            return true;
        }

        return !specialEventIds.Contains(ticket.SpecialEventId.Value);
    }
}
