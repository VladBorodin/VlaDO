using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using VlaDO;

public sealed class PasswordResetCleanupService : BackgroundService
{
    private readonly IServiceProvider _sp;

    public PasswordResetCleanupService(IServiceProvider sp) => _sp = sp;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await SweepAsync();

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromHours(1), ct);
                await SweepAsync();
            }
            catch (TaskCanceledException) { /* shutdown */ }
        }

        async Task SweepAsync()
        {
            using var scope = _sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DocumentFlowContext>();

            var affected = await db.Database.ExecuteSqlRawAsync(
                """
                DELETE FROM PasswordResetToken
                WHERE ExpiresAt < {0}
                """, DateTime.UtcNow);

            if (affected > 0)
                Console.WriteLine($"[cleanup] удалено {affected} просроченных reset-токенов");
        }
    }
}
