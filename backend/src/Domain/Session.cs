using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Session
{
    public Guid Id { get; set; }
    public Guid Token { get; set; }
    public required Guid UserId { get; set; }
    public required User? User { get; set; }

    public Session()
    {
        Id = Guid.NewGuid();
    }

    [SetsRequiredMembers]
    public Session(User user)
    {
        Id = Guid.NewGuid();
        Token = Guid.NewGuid();
        User = user;
        UserId = user.Id;
    }
}
