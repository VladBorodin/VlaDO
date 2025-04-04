namespace VlaDO
{
    public static class DatabaseInitializer
    {
        public static void EnsureDatabaseCreated(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DocumentFlowContext>();

            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("DatabaseInitializer");

            try
            {
                logger.LogInformation("Проверка базы данных...");

                context.Database.EnsureCreated();

                logger.LogInformation("База данных успешно проверена и создана.");
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка при инициализации базы данных: {ex.Message}");
            }
        }
    }
}
