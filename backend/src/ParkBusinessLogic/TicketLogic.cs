using Domain;
using IDataAccess;

using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class TicketLogic : ITicketLogic
{
    private readonly IRepository<Ticket> _repo;
    private readonly ISpecialEventLogic _specialEventLogic;
    private readonly IUserLogic _userLogic;

    public TicketLogic(IRepository<Ticket> repo, ISpecialEventLogic specialEventLogic, IUserLogic userLogic)
    {
        _repo = repo;
        _specialEventLogic = specialEventLogic;
        _userLogic = userLogic;
    }

    public Ticket GetByQrOrThrow(Guid qr)
    {
        return _repo.Find(t => t.QrCode == qr)
               ?? throw new KeyNotFoundException($"Ticket with QR {qr} not found.");
    }

    public Guid BuyCreateTicket(Guid userId, TicketType type, Guid? specialEventId)
    {
        Guid? evId = type == TicketType.SpecialEvent ? specialEventId
            : null;

        if (type == TicketType.SpecialEvent && evId is null)
        {
            throw new ArgumentException("SpecialEventId is required for SpecialEvent tickets.", nameof(specialEventId));
        }

        if (evId is not null)
        {
            var ev = _specialEventLogic.GetOrThrow(evId.Value);
            var sold = _repo.FindAll(t => t.SpecialEventId == ev.Id).Count;
            _specialEventLogic.EnsureSaleAllowed(ev, sold, DateTime.UtcNow);
        }

        var owner = _userLogic.GetByIdOrThrow(userId);
        var qr = GenerateUniqueQrOrThrow();

        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            QrCode = qr,
            Type = type,
            UserId = userId,
            Owner = owner,
            SpecialEventId = evId,
            VisitDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        _repo.Add(ticket);
        return qr;
    }

    private Guid GenerateUniqueQrOrThrow(int maxAttempts = 5)
    {
        for (var i = 0; i < maxAttempts; i++)
        {
            var candidate = Guid.NewGuid();
            if (_repo.Find(t => t.QrCode == candidate) is null)
            {
                return candidate;
            }
        }

        throw new InvalidOperationException("Could not generate a unique QR. Try again.");
    }

    public bool ValidateDateAndTimeTicket(Guid qrCode, DateTime nowUtc)
    {
        var ticket = GetByQrOrThrow(qrCode);
        return ticket.VisitDate == DateOnly.FromDateTime(nowUtc);
    }
}
