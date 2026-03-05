using Domain;

namespace obligatorio.WebApi.DTO;

public class CreateRewardDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public int CostPoints { get; set; }
    public int QuantityAvailable { get; set; }
    public MembershipLevel? MembershipLevel { get; set; }
}
