using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VlaDO;

public static class DatabaseInitializer
{
    public static void EnsureDatabaseCreated(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<DocumentFlowContext>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("DatabaseInitializer");

        try
        {
            if (ctx.Database.EnsureCreated())
                logger.LogInformation("База данных создана с нуля.");
            else
                logger.LogInformation("Схема БД уже существовала – пропуск создания.");

            // сидеры (Roles, ClientTypes и т.п.)
            // await SeedDataAsync(ctx, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при создании базы данных");
            throw;
        }
    }
}