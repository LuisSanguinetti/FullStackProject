using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/session")]
public class SessionController : ControllerBase
{
    private readonly ISessionLogic _logic;

    public SessionController(ISessionLogic logic)
    {
        _logic = logic;
    }

    [Auth]
    [HttpPut("logout")]
    public IActionResult Logout([FromHeader(Name = "Authorization")] string? authorization)
    {
        var raw = authorization?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
            ? authorization.Substring(7).Trim()
            : authorization?.Trim();

        if (Guid.TryParse(raw, out var token))
        {
            _logic.DeleteSession(token);
        }

        return Ok(new { message = "logged out" });
    }
}
