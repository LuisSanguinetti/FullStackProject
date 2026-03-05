using System.Security.Authentication;
using System.Security.Permissions;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/missions/completions")]
public class MissionCompletionController : ControllerBase
{
    private readonly IMissionCompletionLogic _logic;

    public MissionCompletionController(IMissionCompletionLogic logic)
    {
        _logic = logic;
    }

    [HttpPost("{missionId:guid}")]
    [Auth]
    public ActionResult Create(Guid missionId, [FromBody] Guid scoringStrategyId)
    {
        var userId = (HttpContext.Items.TryGetValue("CurrentUserId", out var v) && v is Guid g)
            ? g
            : throw new InvalidCredentialException();

        var completedAt = DateTime.UtcNow;
        var points = _logic.Register(userId, missionId, completedAt, scoringStrategyId);

        return Ok(points);
    }
}
