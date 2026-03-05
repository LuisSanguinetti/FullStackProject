using System;
using Domain;
using IDataAccess;
using IParkBusinessLogic;

namespace Park.BusinessLogic;

public class UserRoleLogic : IUserRoleLogic
{
    private readonly IRepository<Role> _roleRepo;
    private readonly IRepository<UserRole> _userRoleRepo;

    public UserRoleLogic(IRepository<Role> roleRepo, IRepository<UserRole> userRoleRepo)
    {
        _roleRepo = roleRepo;
        _userRoleRepo = userRoleRepo;
    }

    public void AssignRoleByName(Guid userId, string roleName)
    {
        if(string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentException("Role name required");
        }

        var role = _roleRepo.Find(r => r.Name == roleName)
                   ?? throw new InvalidOperationException($"Role '{roleName}' not found");

        var existing = _userRoleRepo.Find(ur => ur.UserId == userId && ur.RoleId == role.Id);
        if(existing != null)
        {
            throw new InvalidOperationException($"User already has role '{roleName}'");
        }

        _userRoleRepo.Add(new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = role.Id
        });
    }

    public string GetRoleByUserId(Guid userId)
    {
        if (userId == Guid.Empty)
        {
            return "general";
        }

        var admin = _roleRepo.Find(r => r.Name == "admin");
        if (admin != null && _userRoleRepo.Find(ur => ur.UserId == userId && ur.RoleId == admin.Id) != null)
        {
            return "admin";
        }

        var op = _roleRepo.Find(r => r.Name == "operator");
        if (op != null && _userRoleRepo.Find(ur => ur.UserId == userId && ur.RoleId == op.Id) != null)
        {
            return "operator";
        }

        return "general";
    }
}
