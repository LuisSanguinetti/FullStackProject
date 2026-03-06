namespace Domain;

public class UserRole
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public User User { get; set; } = null!;
    public Role Role { get; set; } = null!;

    public UserRole()
    {
    }

    public UserRole(Guid roleId, Guid userId, User user, Role role)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        RoleId = roleId;
        User = user;
        Role = role;
    }
}
