using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/points")]
public class PointsController : ControllerBase
{
    private readonly IPointsHistoryLogic _history;
    private readonly IRepository<ScoringStrategyMeta> _strategies;

    public PointsController(IPointsHistoryLogic history, IRepository<ScoringStrategyMeta> strategies)
    {
        _history = history;
        _strategies = strategies;
    }

    [HttpGet("history")]
    [Auth]
    public IActionResult GetHistory([FromQuery] DateTime? fromUtc, [FromQuery] DateTime? toUtc)
    {
        var me = (HttpContext.Items.TryGetValue("CurrentUserId", out var v) && v is Guid g) ? g : Guid.Empty;
        var items = _history.List(me, fromUtc, toUtc);

        var strategyNames = _strategies
            .FindAll(s => items.Select(i => i.StrategyId).Distinct().Contains(s.Id))
            .ToDictionary(s => s.Id, s => s.Name);

        var dto = items.Select(i => new PointsHistoryItemDto
        {
            AtUtc = i.At,
            Points = i.Points,
            Origin = i.Reason,
            StrategyId = i.StrategyId,
            StrategyName = strategyNames.TryGetValue(i.StrategyId, out var n) ? n : string.Empty
        });

        return Ok(dto);
    }
}
