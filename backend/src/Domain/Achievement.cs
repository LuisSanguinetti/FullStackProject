using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Achievement
{
    public Guid Id { get; set; }

    public required string Name { get; set; }
    public required string Description { get; set; }

    public Achievement()
    {
    }

    [SetsRequiredMembers]
    public Achievement(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
    }
}
