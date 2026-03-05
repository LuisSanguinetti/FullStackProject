using System.ComponentModel.DataAnnotations;

namespace obligatorio.WebApi.DTO;

public class MissionCreateDto
{
    [Required, StringLength(120, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(2000, MinimumLength = 2)]
    public string Description { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "BasePoints must be >= 0.")]
    public int BasePoints { get; set; }
}
