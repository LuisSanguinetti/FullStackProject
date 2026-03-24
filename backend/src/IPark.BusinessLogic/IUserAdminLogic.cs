using Domain;

namespace IParkBusinessLogic;

public interface IUserAdminLogic
{
    User CreateAdmin(string name, string surname, string email, string password, DateOnly dateOfBirth);
    User CreateOperator(string name, string surname, string email, string password, DateOnly dateOfBirth);
    User CreateVisitor(string name, string surname, string email, string password, DateOnly dateOfBirth);
    void Delete(Guid id);
    User GetByIdOrThrow(Guid id);
}
