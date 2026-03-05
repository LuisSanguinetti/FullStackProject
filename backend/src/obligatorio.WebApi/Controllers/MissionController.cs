using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/admin/missions")]
public class MissionController : ControllerBase
{
    private readonly IMissionLogic _logic;

    public MissionController(IMissionLogic logic)
    {
        _logic = logic;
    }

    [HttpPost]
    [Auth(RoleRequired = "admin")]
    public ActionResult Create([FromBody] MissionCreateDto dto)
    {
        var m = _logic.CreateMission(dto.Title, dto.Description, dto.BasePoints);
        var res = new MissionDto
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            BasePoints = m.BasePoints
        };
        return Ok(res);
    }

    [HttpGet("")]
    [Auth]
    public ActionResult<IEnumerable<MissionDto>> List([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var items = _logic.ListPaged(page, pageSize);
        var dto = items.Select(m => new MissionDto
        {
            Id = m.Id,
            Title = m.Title,
            Description = m.Description,
            BasePoints = m.BasePoints
        });
        return Ok(dto);
    }
}
