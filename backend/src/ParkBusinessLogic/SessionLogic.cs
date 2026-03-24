using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic;

public class SessionLogic : ISessionLogic
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IUserLogic _userLogic;

    public SessionLogic(IRepository<Session> sessionRepository, IUserLogic userLogic)
    {
        _sessionRepository = sessionRepository;
        _userLogic = userLogic;
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
        if(session is null)
        {
            return null;
        }

        return _userLogic.GetByIdOrThrow(session.UserId);
    }

    public Guid? Login(string email, string password)
    {
        var normalized = email.Trim().ToLower();
        if(string.IsNullOrWhiteSpace(normalized) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidCredentialsException();
        }

        var existing = _userLogic.CheckCredential(normalized, password);

        var token = CreateSession(existing);
        return token;
    }
}
