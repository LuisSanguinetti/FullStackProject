using Domain;

namespace IParkBusinessLogic;

public interface ISessionLogic
{
    public Guid CreateSession(User user);
    public void DeleteSession(Guid token);
    public User? GetUserBySession(Guid token);
    public Guid? Login(string email, string password);
}
