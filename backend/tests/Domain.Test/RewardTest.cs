namespace Domain.Test;

[TestClass]
public class RewardTest
{
    [TestMethod]
    public void CreateReward_WithCtor_SetsAllFields()
    {
        // arrange
        var name = "Free Drink";
        var desc = "Redeem for one beverage.";
        var cost = 25;

        // act
        var r = new Reward(name, desc, cost);

        // assert
        Assert.AreNotEqual(Guid.Empty, r.Id);
        Assert.AreEqual(name, r.Name);
        Assert.AreEqual(desc, r.Description);
        Assert.AreEqual(cost, r.CostPoints);
    }

    [TestMethod]
    public void Reward_DefaultCtor_WithObjectInitializer_SetsRequiredMembers()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        var r = new Reward
        {
            Id = id,
            Name = "VIP Lounge Access",
            Description = "Access to the VIP lounge for one day.",
            CostPoints = 100,
            QuantityAvailable = 10
        };

        // assert
        Assert.AreEqual(id, r.Id);
        Assert.AreEqual("VIP Lounge Access", r.Name);
        Assert.AreEqual("Access to the VIP lounge for one day.", r.Description);
        Assert.AreEqual(100, r.CostPoints);
    }
}
