using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/reward")]
public sealed class RewardController : ControllerBase
{
    private readonly IRewardLogic _rewardLogic;

    public RewardController(IRewardLogic rewardLogic)
    {
        _rewardLogic = rewardLogic;
    }

    [Auth(RoleRequired = "operator")]
    [HttpPost]
    public IActionResult Create([FromBody] CreateRewardDto dto)
    {
        var reward = _rewardLogic.CreateReward(dto.Name, dto.Description, dto.CostPoints, dto.QuantityAvailable, dto.MembershipLevel);
        return Ok(reward);
    }

    [HttpGet]
    public IActionResult GetAll()
   {
    var rewards = _rewardLogic.GetAllRewards();
    return Ok(rewards);
   }

   [HttpGet("reward")]
   public IActionResult GetRewardId([FromQuery] Guid id)
    {
        var reward = _rewardLogic.GetById(id);
        return Ok(reward);
    }
}
