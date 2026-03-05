using System.Reflection;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace obligatorio.WebApi.Controllers;

[ApiController]
[Route("api/v1/scoring")]
public class PlugInController : ControllerBase
{
    private readonly IPlugInLogic _logic;

    public PlugInController(IPlugInLogic logic)
    {
        _logic = logic;
    }

    [Auth(RoleRequired = "admin")]
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFile plugin, [FromForm] string? name = null)
    {
        if (plugin is null || plugin.Length == 0)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid upload", Detail = "The 'plugin' file is required." });
        }

        try
        {
            await using var stream = plugin.OpenReadStream();
            var meta = await _logic.UploadAsync(stream, plugin.FileName, name);
            return Ok(new { meta.Id, meta.Name, meta.FileName });
        }
        catch (ReflectionTypeLoadException ex)
        {
            var details = string.Join(" | ", ex.LoaderExceptions.Select(e => e.Message));
            return BadRequest(new ProblemDetails { Title = "Plugin load error (dependencies)", Detail = details, Status = 400 });
        }
        catch (BadImageFormatException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid DLL", Detail = ex.Message, Status = 400 });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid plugin", Detail = ex.Message, Status = 400 });
        }
    }
}
