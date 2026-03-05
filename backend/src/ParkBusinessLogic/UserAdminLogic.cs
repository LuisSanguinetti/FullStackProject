using System;
using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Park.BusinessLogic.Exceptions;

namespace Park.BusinessLogic;

public class UserAdminLogic : IUserAdminLogic
{
    private readonly IRepository<User> _userRepo;
    private readonly IUserRoleLogic _userRoleLogic;

    private const string RoleAdmin = "Admin";
    private const string RoleOperator = "Operator";
    private const string RoleVisitor = "Visitor";

    public UserAdminLogic(IRepository<User> userRepo, IUserRoleLogic userRoleLogic)
    {
        _userRepo = userRepo;
        _userRoleLogic = userRoleLogic;
    }

    public User CreateAdmin(string name, string surname, string email, string password, DateOnly dateOfBirth)
    {
        return CreateWithRole(name, surname, email, password, dateOfBirth, MembershipLevel.Standard, RoleAdmin);
    }

    public User CreateOperator(string name, string surname, string email, string password, DateOnly dateOfBirth)
    {
       return CreateWithRole(name, surname, email, password, dateOfBirth, MembershipLevel.Standard, RoleOperator);
    }

    public User CreateVisitor(string name, string surname, string email, string password, DateOnly dateOfBirth, MembershipLevel membership)
    {
        return CreateWithRole(name, surname, email, password, dateOfBirth, membership, RoleVisitor);
    }

    private User CreateWithRole(string name, string surname, string email, string password, DateOnly dob, MembershipLevel membership, string roleName)
    {
        ValidateEmail(email);
        EnsureEmailUnique(email);

        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = new User(name, surname, normalizedEmail, password, dob, membership);
        user = _userRepo.Add(user);

        _userRoleLogic.AssignRoleByName(user.Id, roleName);

        return user;
    }

    private void ValidateEmail(string email)
    {
        if(string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
        {
            throw new ArgumentException("Invalid email", nameof(email));
        }
    }

    private void EnsureEmailUnique(string email)
    {
        var exists = _userRepo.Find(u => u.Email.ToLower() == email.Trim().ToLower());
        if(exists != null)
        {
            throw new DuplicateEmailException(email);
        }
    }

    public void Delete(Guid id)
    {
        var user = _userRepo.Find(u => u.Id == id);
        if (user == null)
        {
            throw new ArgumentException("User not found");
        }

        _userRepo.Delete(id);
    }
}
