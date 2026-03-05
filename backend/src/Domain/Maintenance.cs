using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Maintenance
{
    public Guid Id { get; set; }

    public required Guid AttractionId { get; set; }
    public required Attraction Attraction { get; set; }

    public required DateTime StartAt { get; set; }
    public int DurationMinutes { get; set; }

    public required string Description { get; set; }

    public bool Cancelled { get; private set; }

    public DateTime EndAt => StartAt.AddMinutes(DurationMinutes);

    public Maintenance() { }

    [SetsRequiredMembers]
    public Maintenance(Guid attractionId, Attraction attraction, DateTime startAt, int durationMinutes, string description)
    {
        Id = Guid.NewGuid();
        AttractionId = attractionId;
        Attraction = attraction;
        StartAt = startAt;
        DurationMinutes = durationMinutes;
        Description = description;
    }

    public bool IsActiveAt(DateTime when) => !Cancelled && when >= StartAt && when < EndAt;

    public void Cancel(DateTime when)
    {
        if(Cancelled)
        {
            return;
        }

        if(when > EndAt)
        {
            throw new InvalidOperationException("Cannot cancel a finished maintenance.");
        }

        Cancelled = true;
    }
}
