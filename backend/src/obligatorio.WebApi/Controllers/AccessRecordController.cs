using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/accessrecord")]
public sealed class AccessRecordController : ControllerBase
{
    private readonly IAccessRecordLogic _logic;

    public AccessRecordController(IAccessRecordLogic logic)
    {
        _logic = logic;
    }

   [Auth(RoleRequired = "operator")]
     [HttpPost]
    public IActionResult Create([FromBody] AccessRegisterDto dto)
    {
        var points = _logic.Register(dto.TicketQr, dto.AttractionId, DateTime.UtcNow, dto.ScoringStrategyId);
        return Ok(points);
    }

    [Auth(RoleRequired = "operator")]
    [HttpGet("current")]
    public IActionResult GetCurrentCapacity([FromQuery] Guid attraccionId)
    {
        var capacity = _logic.CheckCurrentCapacity(attraccionId);
        return Ok(capacity);
    }

    [Auth(RoleRequired = "operator")]
    [HttpGet("remaining")]
    public IActionResult GetRemainingPeopleCapacity([FromQuery] Guid attraccionId)
    {
      var capacity = _logic.RemainingPeopleCapacity(attraccionId);
        return Ok(capacity);
    }

    [Auth(RoleRequired = "operator")]
    public IActionResult PutRegisterExit([FromBody] Guid accessRecordId)
    {
        _logic.RegisterExit(accessRecordId, DateTime.UtcNow);
        return Ok();
    }
}
