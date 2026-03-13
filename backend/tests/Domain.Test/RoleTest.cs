using Domain;

namespace Domain.Test;

[TestClass]
public class RoleTest
{
    [TestMethod]
    public void CreateRole()
    {
        // Arrange
        var name = "Admin";

        // Act
        var role = new Role(name);

        // Assert
        Assert.IsNotNull(role);
        Assert.AreNotEqual(Guid.Empty, role.Id);
        Assert.AreEqual(name, role.Name);
    }

    [TestMethod]
    public void Role_DefaultCtor()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Editor";
        var role = new Role();

        // Act
        role.Id = id;
        role.Name = name;

        // Assert
        Assert.AreEqual(id, role.Id);
        Assert.AreEqual(name, role.Name);
    }
}
