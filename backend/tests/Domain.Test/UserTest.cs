namespace Domain.Test;

[TestClass]
public class UserTest
{
    [TestMethod]
    public void CreateUser()
    {
        // Arrange

        var name = "Luis";
        var surname = "test";
        var email = "luis@example.com";
        var password = "Password1";

        // Act

        var user = new User("Luis", "test", "luis@example.com", "Password1", new DateOnly(2000, 1, 1));

        // Assert

        Assert.IsNotNull(user);
        Assert.AreNotEqual(Guid.Empty, user.Id);
        Assert.AreEqual(name, user.Name);
        Assert.AreEqual(surname, user.Surname);
        Assert.AreEqual(email, user.Email);
        Assert.AreEqual(password, user.Password);
    }

    [TestMethod]
    public void User_DefaultCtor()
    {
        // Arrange
        var id = Guid.NewGuid();
        var user = new User("Luis", "test", "luis@example.com", "Password1", new DateOnly(2000, 1, 1));
        var name = "Luis";
        var surname = "Test";
        var email = "luis@example.com";

        // Act
        user.Id = id;
        user.Name = name;
        user.Surname = surname;
        user.Email = email;

        // Assert
        Assert.AreEqual(id, user.Id);
        Assert.AreEqual(name, user.Name);
        Assert.AreEqual(surname, user.Surname);
        Assert.AreEqual(email, user.Email);
    }
}
