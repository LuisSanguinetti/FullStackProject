using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/time")]
public sealed class CustomDateTimeProviderController : ControllerBase
{
    private readonly ICustomDateTimeProvider _dateTimeProvider;

    public CustomDateTimeProviderController(ICustomDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    [HttpPut("set")]
    public IActionResult PutSetTime([FromBody] DateTime customTime)
    {
        _dateTimeProvider.SetCustomTime(customTime);
        return Ok("time updated");
    }

    [HttpGet("current")]
    public IActionResult GetCurrentTime()
    {
        try
        {
            var currentTime = _dateTimeProvider.GetNowUtc();
            return Ok(currentTime);
        }
        catch (Exception)
        {
            return Ok(DateTime.UtcNow);
        }
    }
}
