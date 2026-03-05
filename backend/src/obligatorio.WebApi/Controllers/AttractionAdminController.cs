using System;
using System.Collections.Generic;
using System.Linq;
using Domain;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using obligatorio.WebApi.DTO;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/admin/attractions")]
public class AttractionAdminController : ControllerBase
{
    private readonly IAttractionAdminLogic _logic;
    public AttractionAdminController(IAttractionAdminLogic logic) => _logic = logic;

    [Auth(RoleRequired = "admin")]
    [HttpPost]
    public IActionResult Create([FromBody] AttractionCreateDto dto)
    {
        var type = Enum.Parse<AttractionType>(dto.Type, true);

        var a = _logic.Create(dto.Name, type, dto.MinAge, dto.Capacity, dto.Description, dto.BasePoints);
        return CreatedAtAction(nameof(GetById), new { id = a.Id }, AttractionGetDto.From(a));
    }

    [Auth(RoleRequired = "admin")]
    [HttpGet("{id:guid}")]
    public ActionResult<AttractionGetDto> GetById(Guid id)
    {
        var a = _logic.GetOrThrow(id);
        return AttractionGetDto.From(a);
    }

    [Auth(RoleRequired = "admin")]
    [HttpPut("{id:guid}")]
    public ActionResult<AttractionGetDto> Update(Guid id, [FromBody] AttractionUpdateDto dto)
    {
        var type = Enum.Parse<AttractionType>(dto.Type, true);
        var a = _logic.Update(id, dto.Name, type, dto.MinAge, dto.Capacity, dto.Description, dto.BasePoints, dto.Enabled);
        return AttractionGetDto.From(a);
    }

    [Auth(RoleRequired = "admin")]
    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        _logic.Delete(id);
        return NoContent();
    }

    [Auth(RoleRequired = "admin")]
    [HttpGet]
    public IActionResult List([FromQuery] string? type, [FromQuery] bool? enabled)
    {
        AttractionType? t = type is null ? null : Enum.Parse<AttractionType>(type, true);

        var items = _logic.List(t, enabled)
            .Select(AttractionGetDto.From)
            .ToList();

        return Ok(items);
    }
}
