using System;
using Domain;

namespace obligatorio.WebApi.DTO;

    public class CreateRedemptionDto
    {
        public Guid UserId { get; set; }
        public string RewardName { get; set; } = null!;
        public string RewardDescription { get; set; } = null!;
        public int RewardCostPoints { get; set; }
        public int RewardQuantityAvailable { get; set; }
        public MembershipLevel? RewardMembershipLevel { get; set; }
    }