using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/attraction")]
public sealed class AttractionController : ControllerBase
{
    private readonly IAttractionLogic _attractionLogic;

    public AttractionController(IAttractionLogic attractionLogic)
    {
        _attractionLogic = attractionLogic;
    }

    [Auth(RoleRequired = "admin")]
    [HttpGet("visitors")]
    public ActionResult<int> GetVisitorQuantity(
        [FromQuery] Guid attractionId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var count = _attractionLogic.NumberOfVisits(attractionId, startDate, endDate);
        return count;
    }

    [Auth]
    [HttpGet("list")]
    public ActionResult<IEnumerable<AttractionGetDto>> GetAttractions()
    {
        var attr = _attractionLogic.GetAll();
        return Ok(attr.Select(AttractionGetDto.From));
    }
}
