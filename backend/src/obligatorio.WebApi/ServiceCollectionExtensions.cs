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
        services.AddScoped<IScoringStrategyMetaLogic, ScoringStrategyMetaLogic>();
        services.AddScoped<IAwardPointsLogic, AwardPointsLogic>();
        services.AddScoped<IAttractionLogic, AttractionLogic>();
        services.AddScoped<IAccessRecordLogic, AccessRecordLogic>();
        services.AddScoped<ISpecialEventLogic, SpecialEventLogic>();
        services.AddScoped<ITicketLogic, TicketLogic>();
        services.AddScoped<IParkBusinessLogic.ISystemClock, obligatorio.WebApi.Infra.DefaultSystemClock>();
        services.AddScoped<IParkBusinessLogic.ISpecialEventAdminLogic, Park.BusinessLogic.SpecialEventAdminLogic>();
        services.AddScoped<IParkBusinessLogic.IRankingLogic, Park.BusinessLogic.RankingLogic>();
        services.AddScoped<IParkBusinessLogic.IScoringStrategyQueryLogic, Park.BusinessLogic.ScoringStrategyQueryLogic>();
        services.AddScoped<IParkBusinessLogic.IUserAdminLogic, Park.BusinessLogic.UserAdminLogic>();
        services.AddScoped<IParkBusinessLogic.IUserRoleLogic, Park.BusinessLogic.UserRoleLogic>();
        services.AddScoped<IAttractionAdminLogic, AttractionAdminLogic>();
        services.AddScoped<IIncidentLogic, IncidentLogic>();
        services.AddScoped<IAttractionHelperLogic, AttractionHelperLogic>();
        services.AddScoped<IMissionLogic, MissionLogic>();
        services.AddScoped<IMissionCompletionLogic, MissionCompletionLogic>();
        services.AddScoped<IMaintenanceAdminLogic, MaintenanceAdminLogic>();
        services.AddScoped<IMaintenanceQueryLogic, MaintenanceQueryLogic>();
        services.AddScoped<IPointsHistoryLogic, PointsHistoryLogic>();
        services.AddScoped<IRewardLogic, RewardLogic>();
        services.AddScoped<IRedemptionLogic, RedemptionLogic>();
        services.AddSingleton<ICustomDateTimeProvider, CustomDateTimeProvider>();

        services.AddScoped<IPlugInLogic>(sp =>
        {
            var metaLogic = sp.GetRequiredService<IScoringStrategyMetaLogic>();
            var pluginsRoot = Path.Combine(AppContext.BaseDirectory, "Plugins");
            return new PlugInLogic(metaLogic, pluginsRoot);
        });

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
