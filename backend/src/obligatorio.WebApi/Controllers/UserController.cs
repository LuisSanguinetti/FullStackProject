using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;
namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UserController : ControllerBase
{
    private readonly IUserLogic _userLogic;
    private readonly IUserRoleLogic _userRoleLogic;
    private readonly ISessionLogic _sessionLogic;

    public UserController(IUserLogic userLogic, IUserRoleLogic userRoleLogic, ISessionLogic sessionLogic)
    {
        _userLogic = userLogic;
        _userRoleLogic = userRoleLogic;
        _sessionLogic = sessionLogic;
    }

    [HttpGet]
    public IEnumerable<UserGetDto> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        return _userLogic.GetUsersPage(page, pageSize)
            .Select(UserGetDto.DomToDto)
            .ToList();
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] UserRegisterDto dto)
    {
        _userLogic.RegisterVisitor(UserRegisterDto.DtoToDom(dto));
        return Ok();
    }

    [HttpPost("login")]
    public IActionResult LogIn([FromBody] UserLogInDto dto)
    {
        var token = _userLogic.Login(dto.Email, dto.Password);
        return Ok(new { token });
    }

    [Auth]
    [HttpPut]
    public IActionResult Update([FromBody] UserUpdateDto dto)
    {
        // gets the id of the active user
        var me = (HttpContext.Items.TryGetValue("CurrentUserId", out var v) && v is Guid g) ? g : Guid.Empty;
        _userLogic.EditProfile(me, dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth);
        return Ok(new { message = "user updated" });
    }

    [HttpGet("role")]
    public IActionResult GetMyRole([FromHeader(Name = "Authorization")] string? authorization)
    {
        var raw = authorization?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true
            ? authorization.Substring(7).Trim()
            : authorization?.Trim();

        if (!Guid.TryParse(raw, out var token))
        {
            return Ok(new { role = "general" });
        }

        var user = _sessionLogic.GetUserBySession(token);
        var role = user is null ? "general" : _userRoleLogic.GetRoleByUserId(user.Id);
        return Ok(new { role });
    }

     [HttpGet("user")]
     public IActionResult GetUserId([FromQuery] Guid id)
    {
        var user = _userLogic.GetByIdOrThrow(id);
        return Ok(user);
    }
}
