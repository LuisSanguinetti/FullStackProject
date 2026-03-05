using System;
using System.ComponentModel.DataAnnotations;
using Domain;

namespace obligatorio.WebApi.DTO;

public class AttractionCreateDto
{
    [Required, MinLength(2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int MinAge { get; set; }

    [Range(1, int.MaxValue)]
    public int Capacity { get; set; }

    [Required, MinLength(2)]
    public string Description { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int BasePoints { get; set; } = 0;
}

public class AttractionUpdateDto : AttractionCreateDto
{
    public bool? Enabled { get; set; }
}

public record AttractionGetDto(Guid Id, string Name, string Type, int MinAge, int Capacity, string Description, bool Enabled, int BasePoints)
{
    public static AttractionGetDto From(Attraction a) =>
        new(a.Id, a.Name, a.Type.ToString(), a.MinAge, a.MaxCapacity, a.Description, a.Enabled, a.BasePoints);
}
