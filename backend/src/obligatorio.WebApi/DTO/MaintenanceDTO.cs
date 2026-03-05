namespace obligatorio.WebApi.DTO;

public class MaintenanceCreateDto
{
    public Guid AttractionId { get; set; }
    public DateTime StartAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public required string Description { get; set; }
}

public class MaintenanceDto
{
    public Guid Id { get; set; }
    public Guid AttractionId { get; set; }
    public string AttractionName { get; set; } = string.Empty;
    public DateTime StartAtUtc { get; set; }
    public int DurationMinutes { get; set; }
    public DateTime EndAtUtc { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Cancelled { get; set; }
}
