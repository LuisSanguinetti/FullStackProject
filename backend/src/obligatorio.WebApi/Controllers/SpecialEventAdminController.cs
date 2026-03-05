using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/admin/events")]
public class SpecialEventAdminController : ControllerBase
{
    private readonly ISpecialEventAdminLogic _logic;
    public SpecialEventAdminController(ISpecialEventAdminLogic logic) => _logic = logic;

    [HttpPost]
    public IActionResult Create([FromBody] SpecialEventCreateDto dto)
    {
        if(!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if(!DateTime.TryParse(dto.Start, out var start) || !DateTime.TryParse(dto.End, out var end))
        {
            return BadRequest("Invalid date format");
        }

        try
        {
            var ev = _logic.Create(dto.Name, start.ToUniversalTime(), end.ToUniversalTime(), dto.Capacity, dto.ExtraPrice, dto.AttractionIds);
            return CreatedAtAction(nameof(GetById), new { id = ev.Id }, SpecialEventGetDto.From(ev));
        }
        catch(ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public ActionResult<SpecialEventGetDto> GetById(Guid id)
    {
        var ev = _logic.List().FirstOrDefault(e => e.Id == id);
        return ev is null ? NotFound() : SpecialEventGetDto.From(ev);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        try
        {
            _logic.Delete(id);
            return NoContent();
        }
        catch(KeyNotFoundException)
        {
            return NotFound();
        }
        catch(InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpGet]
    public IEnumerable<SpecialEventGetDto> List()
        => _logic.List().Select(SpecialEventGetDto.From);
}
