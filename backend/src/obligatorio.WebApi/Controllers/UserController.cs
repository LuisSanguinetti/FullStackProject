using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;
namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UserController : ControllerBase
{
    // pasar a solo una logica por controller
    private readonly IUserLogic _userLogic;
    private readonly IUserRoleLogic _userRoleLogic;

    public UserController(IUserLogic userLogic, IUserRoleLogic userRoleLogic)
    {
        _userLogic = userLogic;
        _userRoleLogic = userRoleLogic;
    }

    [Auth]
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

    [Auth]
    [HttpPut]
    public IActionResult Update([FromBody] UserUpdateDto dto)
    {
        var me = (HttpContext.Items.TryGetValue("CurrentUserId", out var v) && v is Guid g) ? g : Guid.Empty;
        _userLogic.EditProfile(me, dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth);
        return Ok(new { message = "user updated" });
    }

    [Auth]
    [HttpGet("role")]
    public IActionResult GetMyRole()
    {
        var me = (HttpContext.Items.TryGetValue("CurrentUserId", out var v) && v is Guid g) ? g : Guid.Empty;

        var role = _userRoleLogic.GetRoleByUserId(me);
        return Ok(new { role });
    }

     [Auth]
     [HttpGet("user")]
     public IActionResult GetUserId([FromQuery] Guid id)
    {
        var user = _userLogic.GetByIdOrThrow(id);
        return Ok(UserGetDto.DomToDto(user));
    }
}
