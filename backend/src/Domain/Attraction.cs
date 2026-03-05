using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Attraction
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public AttractionType Type { get; set; }
    public int MinAge { get; set; }
    public int MaxCapacity { get; set; }
    public bool Enabled { get; set; } = true;
    public required string Description { get; set; }
    public int BasePoints { get; set; }

    public Attraction()
    {
    }

    [SetsRequiredMembers]
    public Attraction(string name, AttractionType type, int minAge, int maxCapacity, string description, int basePoints)
    {
        Id = Guid.NewGuid();
        Name = name;
        Type = type;
        MinAge = minAge;
        MaxCapacity = maxCapacity;
        Description = description;
        BasePoints = basePoints;
    }
}
