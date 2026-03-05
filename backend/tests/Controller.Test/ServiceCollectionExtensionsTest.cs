using DataAccess;
using Domain;
using FluentAssertions;
using IDataAccess;
using IParkBusinessLogic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using obligatorio.WebApi;

namespace Controller.Test;

[TestClass]
public class ServiceCollectionExtensionsTest
{
    [TestMethod]
    public void Smoke_Covers_AddDataAccess_AddBusiness_AddApiDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(localdb)\\MSSQLLocalDB;Database=ObliTest;Trusted_Connection=True;TrustServerCertificate=True;"
            })
            .Build();

        // Act
        services.AddDataAccess(config);
        services.AddBusinessServices();
        services.AddApiDefaults();
        services.AddLogging();
        services.AddApiDefaults();

        var sp = services.BuildServiceProvider();

        // Assert
        using var scope = sp.CreateScope();

        scope.ServiceProvider.GetRequiredService<IRepository<User>>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<IUserLogic>().Should().NotBeNull();
        scope.ServiceProvider.GetRequiredService<IPlugInLogic>().Should().NotBeNull();
        scope.ServiceProvider.GetService<IActionResultExecutor<ObjectResult>>().Should().NotBeNull();
    }
}
