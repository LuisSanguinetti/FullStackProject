using Domain;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/Ticket")]
public class TicketController : ControllerBase
{
    private readonly ITicketLogic _logic;

    public TicketController(ITicketLogic logic)
    {
        _logic = logic;
    }

    // red
    [Auth]
    [HttpPost]
    public IActionResult Buy([FromBody] TicketPurchaseDto dto)
    {
        var me = (HttpContext.Items.TryGetValue("CurrentUserId", out var v) && v is Guid g) ? g : Guid.Empty;
        var qr = _logic.BuyCreateTicket(me, dto.Type, dto.SpecialEventId);
        return Ok(new { Qr = qr });
    }
}
