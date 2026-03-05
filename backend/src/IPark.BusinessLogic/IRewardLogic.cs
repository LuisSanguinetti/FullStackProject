using Domain;

namespace IParkBusinessLogic;
public interface IRewardLogic
{
    public Reward CreateReward(string name, string description, int costPoints, int quantityAvailable, MembershipLevel? membershipLevel = null);
    public IEnumerable<Reward> GetAllRewards();
    public Reward? GetById(Guid id);
    public void DeductAvailable(Guid id);
}