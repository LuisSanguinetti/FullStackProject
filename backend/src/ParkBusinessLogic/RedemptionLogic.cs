using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic;

public class RedemptionLogic : IRedemptionLogic
{
    private readonly IRepository<Redemption> _redemptionRepository;
    private readonly IUserLogic _users;
    private readonly IRewardLogic _reward;

    public RedemptionLogic(IRepository<Redemption> redemptionRepository, IUserLogic users, IRewardLogic reward)
    {
        _redemptionRepository = redemptionRepository;
        _users = users;
        _reward = reward;
    }

    public void Redeem(Guid userId, Guid rewardId)
    {
        Reward reward = _reward.GetById(rewardId);
        var user = _users.GetByIdOrThrow(userId);

        if(user.Points <= reward.CostPoints)
        {
            throw new PointsAreMissingException(user.Name);
        }

        if(reward.QuantityAvailable <= 0)
        {
            throw new TheRewardIsNotAvailable(reward.Name);
        }

        _users.DeductPoints(userId, reward.CostPoints);

        _reward.DeductAvailable(reward.Id);

        var redemption = new Redemption
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            RewardId = reward.Id,
            Reward = reward,
            DateClaimed = DateTime.Now,
            CostPoints = reward.CostPoints
        };

        _redemptionRepository.Add(redemption);
    }

    public IEnumerable<Redemption> GetAllRedemptions()
    {
        return _redemptionRepository.FindAll();
    }
}