using System.Linq;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/admin/ranking")]
public class RankingAdminController : ControllerBase
{
    private readonly IRankingLogic _logic;
    public RankingAdminController(IRankingLogic logic) => _logic = logic;

    [HttpGet("daily")]
    public ActionResult GetDaily()
    {
        var list = _logic.GetDailyTop(10);
        return Ok(list.Select(e => new RankingEntryDto(e.UserId, e.Name, e.Surname, e.Email, e.TotalPoints )));
    }
}
