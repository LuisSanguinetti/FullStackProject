using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/incident")]
public sealed class IncidentController : ControllerBase
{
    private readonly IIncidentLogic _incidentLogic;

    public IncidentController(IIncidentLogic incidentLogic)
    {
        _incidentLogic = incidentLogic;
    }

    [Auth(RoleRequired = "admin,operator")]
    [HttpPost("CreateIncident")]
    public IActionResult PostCreateIncident([FromBody] CreateIncidentDto dto)
    {
        _incidentLogic.CreateIncident(dto.Description, dto.ReportedAt, dto.AttractionId);
        return Ok();
    }

    [Auth(RoleRequired = "admin,operator")]
    [HttpPut]
    public IActionResult PutResolveIncident([FromBody] Guid incidentId)
    {
        _incidentLogic.ResolveIncident(incidentId);
        return Ok();
    }

    [Auth(RoleRequired = "admin,operator")]
    [HttpGet]
    public IActionResult GetAllIncident()
    {
        var result = _incidentLogic.GetAllIncident();
        return Ok(result);
    }
}
