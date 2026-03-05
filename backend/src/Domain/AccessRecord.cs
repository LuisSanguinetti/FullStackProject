using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class AccessRecord
{
    public Guid Id { get; set; }

    public required DateTime InAt { get; set; }
    public DateTime? OutAt { get; set; }

    public Guid TicketId { get; set; }
    public required Ticket Ticket { get; set; }

    public Guid AttractionId { get; set; }
    public required Attraction Attraction { get; set; }

    public Guid? SpecialEventId { get; set; }
    public SpecialEvent? SpecialEvent { get; set; }
    public int? Points { get; set; }

    public AccessRecord()
    {
    }

    [SetsRequiredMembers]
    public AccessRecord(DateTime inAt, Ticket ticket, Guid ticketId, Attraction attraction, Guid attractionId, int points, SpecialEvent? specialEvent = null, Guid? specialEventId = null)
    {
        Id = Guid.NewGuid();
        InAt = inAt;

        Ticket = ticket;
        TicketId = ticketId;

        Attraction = attraction;
        AttractionId = attractionId;

        SpecialEvent = specialEvent;
        SpecialEventId = specialEventId;
        Points = points;
    }

    public void CheckOut(DateTime when)
    {
        if (when < InAt)
        {
            throw new ArgumentException("Checkout time cannot be before check-in time.", nameof(when));
        }

        OutAt = when;
    }
}
