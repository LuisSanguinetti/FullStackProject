using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Incident
{
    public Guid Id { get; set; }
    public required Guid AttractionId { get; set; }
    public required Attraction Attraction { get; set; }
    public required string Description { get; set; }
    public required DateTime ReportedAt { get; set; }
    public bool Resolved { get; set; } = false;
    public DateTime ResolvedAt { get; set; }

    public Incident()
    {
    }

    [SetsRequiredMembers]
    public Incident(string description, DateTime reportedAt, Attraction attraction, Guid attractionId)
    {
        Id = Guid.NewGuid();
        Description = description;
        ReportedAt = reportedAt;
        Attraction = attraction;
        AttractionId = attractionId;
    }

    public void Resolve(DateTime when)
    {
        if(Resolved)
        {
            throw new InvalidOperationException("Incident already resolved.");
        }

        if(when < ReportedAt)
        {
            throw new ArgumentException("Resolve time cannot be before ReportedAt.", nameof(when));
        }

        Resolved = true;
        ResolvedAt = when;
    }
}
