using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class UnlockLog
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public required User User { get; set; }

    public Guid AchievementId { get; set; }
    public required Achievement Achievement { get; set; }

    public DateTime DateUnlocked { get; set; }

    public UnlockLog()
    {
    }

    [SetsRequiredMembers]
    public UnlockLog(Guid userId, Guid achievementId, DateTime at, User user, Achievement achievement)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AchievementId = achievementId;
        DateUnlocked = at;
        User = user;
        Achievement = achievement;
    }
}
