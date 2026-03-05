using System.Linq.Expressions;
using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class AttractionAdminLogic : IAttractionAdminLogic
{
    private readonly IRepository<Attraction> _attrRepo;
    private readonly IIncidentLogic _incidentLogic;
    private readonly ISpecialEventLogic _eventLogic;

    public AttractionAdminLogic(IRepository<Attraction> attrRepo, IIncidentLogic incidentLogic, ISpecialEventLogic eventLogic)
    {
        _attrRepo = attrRepo;
        _incidentLogic = incidentLogic;
        _eventLogic = eventLogic;
    }

    public Attraction Create(string name, AttractionType type, int minAge, int capacity, string description, int basePoints = 0)
    {
        Validate(type, minAge, capacity, name, description);
        var a = new Attraction(name, type, minAge, capacity, description, basePoints) { Enabled = true };
        return _attrRepo.Add(a);
    }

    public Attraction Update(Guid id, string name, AttractionType type, int minAge, int capacity, string description, int basePoints = 0, bool? enabled = null)
    {
        Validate(type, minAge, capacity, name, description);
        var a = _attrRepo.Find(x => x.Id == id) ?? throw new KeyNotFoundException($"Attraction {id} not found");

        a.Name = name;
        a.Type = type;
        a.MinAge = minAge;
        a.MaxCapacity = capacity;
        a.Description = description;
        a.BasePoints = basePoints;
        if(enabled.HasValue)
        {
            a.Enabled = enabled.Value;
        }

        return _attrRepo.Update(a)!;
    }

    public void Delete(Guid id)
    {
        _ = _attrRepo.Find(x => x.Id == id) ?? throw new KeyNotFoundException($"Attraction {id} not found");

        if(_incidentLogic.HasActiveIncidents(id))
        {
            throw new InvalidOperationException("Cannot delete attraction with active incidents");
        }

        if (_eventLogic.IsAttractionReferenced(id))
        {
            throw new InvalidOperationException("Cannot delete attraction referenced by special events");
        }

        _attrRepo.Delete(id);
    }

    public IEnumerable<Attraction> List(AttractionType? type = null, bool? enabled = null)
    {
        Expression<Func<Attraction, bool>>? filter = null;
        if(type.HasValue && enabled.HasValue)
        {
            filter = a => a.Type == type.Value && a.Enabled == enabled.Value;
        }
        else if(type.HasValue)
        {
            filter = a => a.Type == type.Value;
        }
        else if(enabled.HasValue)
        {
            filter = a => a.Enabled == enabled.Value;
        }

        return _attrRepo.FindAll(filter ?? (_ => true));
    }

    public void SetEnabled(Guid id, bool enabled)
    {
        var a = _attrRepo.Find(x => x.Id == id) ?? throw new KeyNotFoundException($"Attraction {id} not found");
        a.Enabled = enabled;
        _attrRepo.Update(a);
    }

    private static void Validate(AttractionType type, int minAge, int capacity, string name, string description)
    {
        if(!Enum.IsDefined(typeof(AttractionType), type))
        {
            throw new ArgumentException("Invalid type", nameof(type));
        }

        if(capacity <= 0)
        {
            throw new ArgumentException("Capacity must be > 0", nameof(capacity));
        }

        if(minAge < 0)
        {
            throw new ArgumentException("MinAge must be >= 0", nameof(minAge));
        }

        if(string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Name is required", nameof(name));
        }

        if(string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Description is required", nameof(description));
        }
    }

    public Attraction GetOrThrow(Guid id) =>
        _attrRepo.Find(x => x.Id == id) ?? throw new KeyNotFoundException($"Attraction {id} not found");
}
