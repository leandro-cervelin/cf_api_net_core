using CF.Customer.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CF.Api.Startup;

public static class DatabaseMigration
{
    /// <summary>Applies pending EF Core migrations on startup when Database:MigrateOnStartup is true.</summary>
    public static WebApplication MigrateDatabaseIfEnabled(this WebApplication app)
    {
        // Off by default: with several instances, startup migrations race each other and the app login needs DDL
        // rights. Deployments run the migration bundle as a separate step instead (see README).
        if (!app.Configuration.GetValue("Database:MigrateOnStartup", false)) return app;

        using var serviceScope = app.Services.CreateScope();

        var context = serviceScope.ServiceProvider.GetRequiredService<CustomerContext>();

        context.Database.Migrate();

        return app;
    }
}
