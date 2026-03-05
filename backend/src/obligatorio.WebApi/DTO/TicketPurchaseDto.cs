using Domain;

namespace obligatorio.WebApi.DTO;

public class TicketPurchaseDto
{
    public TicketType Type { get; init; }
    public Guid? SpecialEventId { get; init; }
}
