using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class PointsAward
{
    public Guid Id { get; set; }

    // Recipient
    public Guid UserId { get; set; }
    public required User User { get; set; }

    public int Points { get; set; }
    public required string Reason { get; set; }
    public Guid StrategyId { get; set; }

    public DateTime At { get; set; }

    public PointsAward()
    {
    }

    [SetsRequiredMembers]
    public PointsAward(Guid userId, User user, int points, string reason, Guid strategyId, DateTime at)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        User = user;
        Points = points;
        Reason = reason;
        StrategyId = strategyId;
        At = at;
    }
}
