using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic;

public class SessionLogic : ISessionLogic
{
    private readonly IRepository<Session> _sessionRepository;

    public SessionLogic(IRepository<Session> sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public Guid CreateSession(User user)
    {
        var session = new Session(user);
        return _sessionRepository.Add(session).Token;
    }

    public void DeleteSession(Guid token)
    {
        var existing = _sessionRepository.Find(s => s.Token == token);
        if (existing is null)
        {
            throw new ArgumentException("invalid token");
        }

        _sessionRepository.Delete(existing.Id);
    }

    public User? GetUserBySession(Guid token)
    {
        var session = _sessionRepository.Find(s => s.Token == token);
        return session?.User;
    }
}
