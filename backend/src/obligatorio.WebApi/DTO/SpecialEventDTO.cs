using System;
using System.ComponentModel.DataAnnotations;
using Domain;

namespace obligatorio.WebApi.DTO;

public class SpecialEventCreateDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Start { get; set; } = string.Empty;
    [Required]
    public string End { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ExtraPrice { get; set; }

    public Guid[] AttractionIds { get; set; } = Array.Empty<Guid>();
}

public record SpecialEventGetDto(Guid Id, string Name, DateTime Start, DateTime End, int Capacity, decimal ExtraPrice, Guid[] AttractionIds)
{
    public static SpecialEventGetDto From(SpecialEvent e) =>
        new(e.Id, e.Name, e.StartDate, e.EndDate, e.Capacity, e.ExtraPrice, e.Attractions.Select(a => a.Id).ToArray());
}
