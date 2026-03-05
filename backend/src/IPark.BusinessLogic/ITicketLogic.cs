using Domain;

namespace IParkBusinessLogic;

public interface ITicketLogic
{
    Ticket GetByQrOrThrow(Guid qr);

    public Guid BuyCreateTicket(Guid userId, TicketType type, Guid? specialEventId);
    public bool ValidateDateAndTimeTicket(Guid qrCode, DateTime nowUtc);
}
