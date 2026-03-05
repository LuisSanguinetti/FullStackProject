using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/redemption")]
public sealed class RedemptionController : ControllerBase
{
    private readonly IRedemptionLogic _redemptionLogic;

    public RedemptionController(IRedemptionLogic redemptionLogic)
    {
        _redemptionLogic = redemptionLogic;
    }

    [HttpPost]
    [Auth]
    public IActionResult Redeem([FromQuery] Guid rewardId)
    {
        var userId = (HttpContext.Items.TryGetValue("CurrentUserId", out var v) && v is Guid g) ? g : Guid.Empty;

        _redemptionLogic.Redeem(userId, rewardId);
        return Ok();
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var redemptions = _redemptionLogic.GetAllRedemptions();
        return Ok(redemptions);
    }
}