using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Mission
{
    public Guid Id { get; set; }

    public required string Title { get; set; }
    public required string Description { get; set; }

    public int BasePoints { get; set; }

    public Mission()
    {
    }

    [SetsRequiredMembers]
    public Mission(string title, string description, int basePoints)
    {
        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        BasePoints = basePoints;
    }
}
