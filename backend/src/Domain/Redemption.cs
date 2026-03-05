using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Redemption
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public required User User { get; set; }

    public Guid RewardId { get; set; }
    public required Reward Reward { get; set; }

    public DateTime DateClaimed { get; set; }
    public int CostPoints { get; set; }

    public Redemption()
    {
    }

    [SetsRequiredMembers]
    public Redemption(Guid userId, Guid rewardId, DateTime at, int costPoints, Reward reward, User user)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RewardId = rewardId;
        DateClaimed = at;
        CostPoints = costPoints;
        Reward = reward;
        User = user;
    }
}
