using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VlaDO;

/// <summary>
/// Класс для инициализации базы данных и применения миграций при запуске приложения.
/// </summary>
public static class DatabaseInitializer
{
    /// <summary>
    /// Проверяет подключение к базе данных и применяет миграции, если это необходимо.
    /// </summary>
    /// <param name="services">Контейнер сервисов, из которого извлекается контекст и логгер.</param>
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
                logger.LogInformation("База данных не существует или недоступна. Применяются миграции...");
            }
            else
            {
                logger.LogInformation("База данных существует. Применяются миграции (если есть)...");
            }

            ctx.Database.Migrate();
            logger.LogInformation("Миграции успешно применены");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при проверке или миграции базы данных");
            throw;
        }
    }
}