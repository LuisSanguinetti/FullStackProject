using DataAccess;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.EntityFrameworkCore;
using Park.BusinessLogic;

namespace obligatorio.WebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration config)
    {
        var cs = config.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Missing DefaultConnection");
        services.AddDbContext<ObligatorioDbContext>(o => o.UseSqlServer(cs));
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        return services;
    }

    public static IServiceCollection AddBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IUserLogic, UserLogic>();
        services.AddScoped<ISessionLogic, SessionLogic>();
        services.AddScoped<IParkBusinessLogic.IUserAdminLogic, Park.BusinessLogic.UserAdminLogic>();
        services.AddScoped<IParkBusinessLogic.IUserRoleLogic, Park.BusinessLogic.UserRoleLogic>();

        return services;
    }

    public static IServiceCollection AddApiDefaults(this IServiceCollection services)
    {
        services.AddControllers(o => o.Filters.Add<Filters.GlobalExceptionFilter>());
        services.AddProblemDetails();
        services.AddLogging();
        return services;
    }
}
