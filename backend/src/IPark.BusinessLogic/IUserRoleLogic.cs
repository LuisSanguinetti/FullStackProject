namespace IParkBusinessLogic;

public interface IUserRoleLogic
{
    void AssignRoleByName(Guid userId, string roleName);
    string GetRoleByUserId(Guid userId);
}
