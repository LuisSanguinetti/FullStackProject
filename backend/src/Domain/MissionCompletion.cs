using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class MissionCompletion
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public Guid MissionId { get; set; }
    public Mission? Mission { get; set; }

    public DateTime DateCompleted { get; set; }
    public int Points { get; set; }

    public MissionCompletion()
    {
    }

    [SetsRequiredMembers]
    public MissionCompletion(Guid userId, Guid missionId, DateTime at, int points)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        MissionId = missionId;
        DateCompleted = at;
        Points = points;
    }
}
