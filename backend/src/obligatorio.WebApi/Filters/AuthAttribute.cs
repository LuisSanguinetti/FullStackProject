using Domain;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc.Filters;
using Park.BusinessLogic.Exceptions;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class AuthAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string? RoleRequired { get; init; }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var http = context.HttpContext;
        var header = http.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(header))
        {
            throw new InvalidCredentialsException(); // 401
        }

        var tokenText = header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header.Substring("Bearer ".Length).Trim()
            : header.Trim();

        if (!Guid.TryParse(tokenText, out var token))
        {
            throw new InvalidCredentialsException(); // 401
        }

        // get implementation
        var sessions = http.RequestServices.GetRequiredService<ISessionLogic>();
        var user = sessions.GetUserBySession(token);
        if (user is null)
        {
            throw new InvalidCredentialsException(); // 401
        }

        http.Items["CurrentUserId"] = user.Id;

        if (!string.IsNullOrEmpty(RoleRequired))
        {
            var requiredRoles = RoleRequired
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var userRolesRepo = http.RequestServices.GetRequiredService<IRepository<UserRole>>();
            var hasRole = userRolesRepo.Find(
                ur => ur.UserId == user.Id && requiredRoles.Contains(ur.Role.Name)
            ) is not null;

            if (!hasRole)
            {
                throw new InvalidCredentialsException();
            }
        }

        await Task.CompletedTask;
    }
}
