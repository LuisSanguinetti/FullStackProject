using System;
using Domain;

namespace IParkBusinessLogic;

public interface IUserAdminLogic
{
    User CreateAdmin(string name, string surname, string email, string password, DateOnly dateOfBirth);
    User CreateOperator(string name, string surname, string email, string password, DateOnly dateOfBirth);
    User CreateVisitor(string name, string surname, string email, string password, DateOnly dateOfBirth, MembershipLevel membership);
    void Delete(Guid id);
}
