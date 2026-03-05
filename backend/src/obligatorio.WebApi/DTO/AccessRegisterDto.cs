namespace obligatorio.WebApi.DTO;

public sealed class AccessRegisterDto
{
    public Guid TicketQr { get; init; }
    public Guid AttractionId { get; init; }
    public Guid UserId { get; init; }
    public Guid ScoringStrategyId { get; set; }
}
