using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Ticket
{
    public Guid Id { get; set; }
    public required Guid QrCode { get; set; }

    public DateOnly VisitDate { get; set; }
    public required TicketType Type { get; set; }

    public required User Owner { get; set; }
    public Guid UserId { get; set; }

    public Guid? SpecialEventId { get; set; }
    public SpecialEvent? SpecialEvent { get; set; }

    public Ticket()
    {
    }

    [SetsRequiredMembers]
    public Ticket(Guid qrCode, DateOnly visitDate, TicketType type, User owner, Guid userId, SpecialEvent specialEvent, Guid? specialEventId)
    {
        Id = Guid.NewGuid();
        QrCode = qrCode;
        Type = type;
        VisitDate = visitDate;
        Owner = owner;
        UserId = userId;
        SpecialEvent = specialEvent;
        SpecialEventId = specialEventId;
    }
}
