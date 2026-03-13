using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic;

public class UserLogic : IUserLogic
{
    private readonly IRepository<User> _userRepository;

    public UserLogic(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public IEnumerable<User> GetUsersPage(int page, int pageSize)
    {
        if(page < 1)
        {
            page = 1;
        }

        if(pageSize <= 0)
        {
            pageSize = 10;
        }

        if(pageSize > 15)
        {
            pageSize = 15;
        }

        return _userRepository.GetPage(
            page,
            pageSize,
            filter: null,
            orderBy: q => q.OrderBy(u => u.Surname)
                .ThenBy(u => u.Name)
                .ThenBy(u => u.Id));
    }

    public void RegisterVisitor(User user)
    {
        if (!IsEmailUnique(user.Email))
        {
            throw new DuplicateEmailException(user.Email);
        }

        _userRepository.Add(user);
    }

    public bool IsEmailUnique(string email)
    {
        var normalized = email.Trim().ToLower();
        return _userRepository.Find(u => u.Email.ToLower() == normalized) is null;
    }

    public void EditProfile(Guid activeUserId, string? name, string? surname, string? email, string? password, DateOnly? dateOfBirth)
    {
        var changed = false;

        if (activeUserId == Guid.Empty)
        {
            throw new InvalidCredentialsException();
        }

        var user = _userRepository.Find(u => u.Id == activeUserId)
                   ?? throw new KeyNotFoundException($"User with id: {activeUserId} not found");

        if (!string.IsNullOrWhiteSpace(name))
        {
            user.Name = name;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(surname))
        {
            user.Surname = surname;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(password))
        {
            user.Password = password;
            changed = true;
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            email = email.Trim();
            if (IsEmailUsedByAnother(user.Id, email))
            {
                throw new DuplicateEmailException(email);
            }

            user.Email = email;
            changed = true;
        }

        if (dateOfBirth is not null)
        {
            user.DateOfBirth = dateOfBirth.Value;
            changed = true;
        }

        if(changed)
        {
            _userRepository.Update(user);
        }
    }

    public User GetByIdOrThrow(Guid id)
    {
        return _userRepository.Find(u => u.Id == id)
               ?? throw new KeyNotFoundException($"User with id: {id} not found");
    }

    public int CalculateAge(Guid userId)
    {
        User user = GetByIdOrThrow(userId);
        var today = DateOnly.FromDateTime(DateTime.Now);
        var age = today.Year - user.DateOfBirth.Year;
        if(today < user.DateOfBirth.AddYears(age))
        {
           age--;
        }

        return age;
    }

    public User GetOrThrow(Guid id)
    {
        return _userRepository.Find(u => u.Id == id)
               ?? throw new KeyNotFoundException($"User with id: {id} not found");
    }

    private bool IsEmailUsedByAnother(Guid me, string email)
    {
        return _userRepository.Find(u => u.Email == email && u.Id != me) is not null;
    }

    public User CheckCredential(string email, string password)
    {
        var normalized = email.Trim().ToLower();
        if(string.IsNullOrWhiteSpace(normalized) || string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidCredentialsException();
        }

        var existing = _userRepository.Find(u => u.Email.ToLower() == normalized && u.Password == password);
        if(existing is null)
        {
            throw new InvalidCredentialsException();
        }

        return existing;
    }
}
