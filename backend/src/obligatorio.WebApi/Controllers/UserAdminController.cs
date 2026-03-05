using System;
using Domain;
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

    [HttpPost("admins")]
    public IActionResult CreateAdmin([FromBody] AdminCreateDto dto)
    {
        var u = _logic.CreateAdmin(dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth);
        return CreatedAtAction(nameof(GetById), new { id = u.Id },
            new UserGetDtos(u.Id, u.Name, u.Surname, u.Email, "Admin", u.Membership));
    }

    [HttpPost("operators")]
    public IActionResult CreateOperator([FromBody] OperatorCreateDto dto)
    {
        var u = _logic.CreateOperator(dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth);
        return CreatedAtAction(nameof(GetById), new { id = u.Id },
            new UserGetDtos(u.Id, u.Name, u.Surname, u.Email, "Operator", u.Membership));
    }

    [HttpPost("visitors")]
    public IActionResult CreateVisitor([FromBody] VisitorCreateDto dto)
    {
        var u = _logic.CreateVisitor(dto.Name, dto.Surname, dto.Email, dto.Password, dto.DateOfBirth, dto.Membership);
        return CreatedAtAction(nameof(GetById), new { id = u.Id },
            new UserGetDtos(u.Id, u.Name, u.Surname, u.Email, "Visitor", u.Membership));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<UserGetDtos> GetById(Guid id) => NotFound();

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _logic.Delete(id);
        return NoContent();
    }
}
