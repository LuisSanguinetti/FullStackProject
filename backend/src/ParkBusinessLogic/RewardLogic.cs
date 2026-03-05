using System.Runtime.CompilerServices;
using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class RewardLogic : IRewardLogic
{
    private readonly IRepository<Reward> _rewardRepository;

    public RewardLogic(IRepository<Reward> rewardRepository)
    {
        _rewardRepository = rewardRepository;
    }

    public Reward CreateReward(string name, string description, int costPoints, int quantityAvailable, MembershipLevel? membershipLevel = null)
    {
        var reward = new Reward
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CostPoints = costPoints,
            QuantityAvailable = quantityAvailable,
            MembershipLevel = membershipLevel
        };

        _rewardRepository.Add(reward);
        return reward;
    }

    public IEnumerable<Reward> GetAllRewards()
    {
        return _rewardRepository.FindAll();
    }

    public Reward? GetById(Guid id)
    {
        return _rewardRepository.Find(r => r.Id == id);
    }

    public void DeductAvailable(Guid id)
    {
       var reward = _rewardRepository.Find(r => r.Id == id);
       reward.QuantityAvailable--;
        _rewardRepository.Update(reward);
    }
}
