using Domain;

namespace IParkBusinessLogic;

public interface IUserLogic
{
    public IEnumerable<User> GetUsersPage(int page, int pageSize);
    public void RegisterVisitor(User user);
    public bool IsEmailUnique(string email);
    public Guid? Login(string email, string password);
    public void EditProfile(Guid activeUserId, string? name, string? surname, string? email, string? password, DateOnly? dateOfBirth);
    public User GetByIdOrThrow(Guid id);
    public int CalculateAge(Guid userId);
    User GetOrThrow(Guid id);
}
