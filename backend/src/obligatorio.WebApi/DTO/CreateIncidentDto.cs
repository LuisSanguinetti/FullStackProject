namespace obligatorio.WebApi.DTO;

public class CreateIncidentDto
{
    public string Description { get; set; } = null!;
    public DateTime ReportedAt { get; set; }
    public Guid AttractionId { get; set; }
}
