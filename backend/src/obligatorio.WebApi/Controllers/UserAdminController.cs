using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
public class UserAdminController : ControllerBase
{
    private readonly IUserAdminLogic _logic;
    public UserAdminController(IUserAdminLogic logic) => _logic = logic;

    [Auth(RoleRequired = "admin")]
    [HttpPost("admins")]
    public IActionResult CreateAdmin([FromBody] AdminCreateDto dto)
    {
        var u = _logic.CreateAdmin(dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth);
        return CreatedAtAction(nameof(CreateAdmin), new { id = u.Id }, new UserGetDtos(u.Id, u.Name, u.Surname, u.Email, "Admin"));
    }

    [Auth(RoleRequired = "admin")]
    [HttpPost("operators")]
    public IActionResult CreateOperator([FromBody] OperatorCreateDto dto)
    {
        var u = _logic.CreateOperator(dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth);
        return CreatedAtAction(nameof(CreateOperator), new { id = u.Id }, new UserGetDtos(u.Id, u.Name, u.Surname, u.Email, "Operator"));
    }

    [Auth(RoleRequired = "admin")]
    [HttpPost("visitors")]
    public IActionResult CreateVisitor([FromBody] VisitorCreateDto dto)
    {
        var u = _logic.CreateVisitor(dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth);
        return CreatedAtAction(nameof(CreateVisitor), new { id = u.Id }, new UserGetDtos(u.Id, u.Name, u.Surname, u.Email, "Visitor"));
    }

    [Auth(RoleRequired = "admin")]
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _logic.Delete(id);
        return NoContent();
    }
}
