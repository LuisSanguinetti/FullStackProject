using IParkBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/mantenimientos")]
public class MaintenancesController : ControllerBase
{
    private readonly IMaintenanceAdminLogic _adminLogic;
    private readonly IMaintenanceQueryLogic _queryLogic;

    public MaintenancesController(
        IMaintenanceAdminLogic adminLogic,
        IMaintenanceQueryLogic queryLogic)
    {
        _adminLogic = adminLogic;
        _queryLogic = queryLogic;
    }

    [Auth(RoleRequired = "admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] MaintenanceCreateDto dto)
    {
        var id = await _adminLogic.ScheduleAsync(dto.AttractionId, dto.StartAtUtc, dto.DurationMinutes, dto.Description);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpDelete("{id:guid}")]
    [Auth(RoleRequired = "admin")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        await _adminLogic.CancelAsync(id, DateTime.UtcNow);
        return NoContent();
    }

    [HttpGet]
    [Auth(RoleRequired = "admin")]
    public IActionResult List([FromQuery] Guid? attractionId, [FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
    {
        var result = _queryLogic.List(attractionId, fromUtc, toUtc)
            .Select(m => new MaintenanceDto
            {
                Id = m.Id,
                AttractionId = m.AttractionId,
                AttractionName = m.Attraction.Name,
                StartAtUtc = m.StartAt,
                DurationMinutes = m.DurationMinutes,
                EndAtUtc = m.EndAt,
                Description = m.Description,
                Cancelled = m.Cancelled
            });

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Auth(RoleRequired = "admin")]
    public IActionResult GetById(Guid id)
    {
        var maintenance = _queryLogic.List().FirstOrDefault(x => x.Id == id);
        if(maintenance is null)
        {
            return NotFound();
        }

        var dto = new MaintenanceDto
        {
            Id = maintenance.Id,
            AttractionId = maintenance.AttractionId,
            AttractionName = maintenance.Attraction.Name,
            StartAtUtc = maintenance.StartAt,
            DurationMinutes = maintenance.DurationMinutes,
            EndAtUtc = maintenance.EndAt,
            Description = maintenance.Description,
            Cancelled = maintenance.Cancelled
        };

        return Ok(dto);
    }
}
