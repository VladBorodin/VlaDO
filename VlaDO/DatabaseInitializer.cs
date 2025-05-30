using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            if (!ctx.Database.CanConnect())
            {
                logger.LogInformation("База данных не существует или недоступна. Применяется миграция...");
                ctx.Database.Migrate();
                logger.LogInformation("Миграции успешно применены");
            }
            else
            {
                logger.LogInformation("База данных существует. Применяется миграция (если есть)...");
            }

        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при проверке или миграции базы данных");
            throw;
        }
    }
}