namespace obligatorio.WebApi.DTO;

public class PointsHistoryItemDto
{
    public DateTime AtUtc { get; set; }
    public int Points { get; set; }
    public string Origin { get; set; } = string.Empty;
    public Guid StrategyId { get; set; }
    public string StrategyName { get; set; } = string.Empty;
}
