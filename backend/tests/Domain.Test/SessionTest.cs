namespace Domain.Test;

[TestClass]
public class SessionTest
{
    [TestMethod]
    public void CreateSession_withUserCtor()
    {
        // Arrange
        var user = new User("Luis", "test", "luis@example.com", "Password1", new DateOnly(2000, 1, 1));

        // Act
        var session = new Session(user);

        // Assert
        Assert.AreEqual(user, session.User);
    }

    [TestMethod]
    public void Session_DefaultCtor_AssignProperties()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = Guid.NewGuid();
        var user = new User("Luis", "test", "luis@example.com", "Password1", new DateOnly(2000, 1, 1));
        var session = new Session(user);

        // Act
        session.Id = id;
        session.Token = token;
        session.User = user;
        session.UserId = user.Id;

        // Assert
        Assert.AreSame(user, session.User);
    }
}
