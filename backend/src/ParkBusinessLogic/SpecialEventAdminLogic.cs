using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class SpecialEventAdminLogic : ISpecialEventAdminLogic
{
    private readonly IRepository<SpecialEvent> _eventRepo;
    private readonly IRepository<Attraction> _attrRepo;
    private readonly IRepository<Ticket> _ticketRepo;
    private readonly ISystemClock _clock;

    public SpecialEventAdminLogic(
        IRepository<SpecialEvent> eventRepo,
        IRepository<Attraction> attrRepo,
        IRepository<Ticket> ticketRepo,
        ISystemClock clock)
    {
        _eventRepo = eventRepo;
        _attrRepo = attrRepo;
        _ticketRepo = ticketRepo;
        _clock = clock;
    }

    public SpecialEvent Create(string name, DateTime start, DateTime end, int capacity, decimal extraPrice, IEnumerable<Guid> attractionIds)
    {
        ValidateCreate(name, start, end, capacity);
        var ids = attractionIds?.ToArray() ?? Array.Empty<Guid>();

        var atts = _attrRepo.FindAll(a => ids.Contains(a.Id), Array.Empty<Expression<Func<Attraction, object>>>());
        if(atts.Count != ids.Length || atts.Any(a => !a.Enabled))
        {
            throw new ArgumentException("One or more attractions are missing or disabled", nameof(attractionIds));
        }

        var ev = new SpecialEvent
        {
            Id = Guid.NewGuid(),
            Name = name,
            StartDate = start,
            EndDate = end,
            Capacity = capacity,
            ExtraPrice = extraPrice,
            Attractions = atts.ToList()
        };

        return _eventRepo.Add(ev);
    }

    public void Delete(Guid id)
    {
        var ev = _eventRepo.Find(e => e.Id == id) ?? throw new KeyNotFoundException($"SpecialEvent {id} not found");

        var hasTickets = _ticketRepo.FindAll(t => t.SpecialEventId == id, Array.Empty<Expression<Func<Ticket, object>>>()).Any();

        if(hasTickets)
        {
            throw new InvalidOperationException("Cannot delete event with sold tickets");
        }

        _eventRepo.Delete(id);
    }

    public IEnumerable<SpecialEvent> List()
    {
        return _eventRepo.FindAll(ev => true, e => e.Attractions);
    }

    private void ValidateCreate(string name, DateTime start, DateTime end, int capacity)
    {
        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required", nameof(name));
        }

        if(capacity <= 0)
        {
            throw new ArgumentException("capacity must be > 0", nameof(capacity));
        }

        if(start >= end)
        {
            throw new ArgumentException("start must be before end");
        }

        if(start < _clock.Now())
        {
            throw new ArgumentException("start is in the past relative to system clock");
        }
    }
}
