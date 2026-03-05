using System.ComponentModel.DataAnnotations;

namespace obligatorio.WebApi.DTO;

public class IncidentCreateDto
{
    [Required(ErrorMessage = "A description of the incident is required.")]
    public string Description { get; set; } = string.Empty;
    [Required(ErrorMessage = "a attraction id is required")]
    public Guid AttractionId { get; set; }
}
