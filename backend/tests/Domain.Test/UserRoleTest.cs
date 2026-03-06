namespace Domain.Test;

[TestClass]
public class UserRoleTest
{
    [TestMethod]
    public void CreateUserRole()
    {
        // Arrange
        var user = new User("John", "Doe", "john@example.com", "password123", new DateOnly(2000, 1, 1));
        var role = new Role("Admin");

        // Act
        var userRole = new UserRole(role.Id, user.Id, user, role);

        // Assert
        Assert.IsNotNull(userRole);
        Assert.AreNotEqual(Guid.Empty, userRole.Id);
        Assert.AreEqual(role.Id, userRole.RoleId);
        Assert.AreEqual(user.Id, userRole.UserId);
        Assert.AreEqual(user, userRole.User);
        Assert.AreEqual(role, userRole.Role);
    }

    [TestMethod]
    public void UserRole_DefaultCtor()
    {
        // Arrange
        var ur = new UserRole();
        var id = Guid.NewGuid();
        var user = new User("John", "Doe", "john@example.com", "password123", new DateOnly(2000, 1, 1));
        var role = new Role("Admin");

        // Act
        ur.Id = id;
        ur.UserId = user.Id;
        ur.RoleId = role.Id;
        ur.User = user;
        ur.Role = role;

        // Assert
        Assert.AreEqual(id, ur.Id);
        Assert.AreEqual(user.Id, ur.UserId);
        Assert.AreEqual(role.Id, ur.RoleId);
        Assert.AreSame(user, ur.User);
        Assert.AreSame(role, ur.Role);
    }
}
