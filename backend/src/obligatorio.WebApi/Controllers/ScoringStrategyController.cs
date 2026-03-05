using Domain;
using IParkBusinessLogic;

using Microsoft.AspNetCore.Mvc;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/scoringStrategy")]
public class ScoringStrategyController : ControllerBase
{
    private readonly IScoringStrategyMetaLogic _logic;

    public ScoringStrategyController(IScoringStrategyMetaLogic logic)
    {
        _logic = logic;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _logic.ListAsync(includeDeleted: false);
        var result = items.Select(m => new
        {
            m.Id,
            m.Name,
            m.FileName,
            m.FilePath,
            m.IsActive,
            m.CreatedOn
        }).ToList();

        return Ok(result);
    }

    [Auth(RoleRequired = "admin")]
    [HttpPut("{id:guid}/active")]
    public async Task<IActionResult> Activate(Guid id)
    {
        await _logic.ActivateAsync(id);
        return NoContent();
    }

    [Auth(RoleRequired = "admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id)
    {
        await _logic.SoftDeleteAsync(id);
        return NoContent();
    }

    [HttpGet("active")]
    public IActionResult GetActive()
    {
        var act = _logic.GetActiveOrThrow();
        return Ok(act);
    }
}
