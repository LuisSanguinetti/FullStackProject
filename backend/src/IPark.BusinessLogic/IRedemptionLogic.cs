using Domain;

namespace IParkBusinessLogic;
public interface IRedemptionLogic
{
    public void Redeem(Guid userId, Guid rewardId);
    public IEnumerable<Redemption> GetAllRedemptions();
}
