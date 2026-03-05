using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Reward
{
    public Guid Id { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required int CostPoints { get; set; }

    public required int QuantityAvailable { get; set; }

    public MembershipLevel? MembershipLevel { get; set; }

    public Reward()
    {
    }

    [SetsRequiredMembers]
    public Reward(string name, string description, int costPoints)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CostPoints = costPoints;
    }
}
