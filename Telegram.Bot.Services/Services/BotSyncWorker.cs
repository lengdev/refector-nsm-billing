using Telegram.Bot.Services.Services;

public class BotSyncWorker : BackgroundService
{
    private readonly BotManager _botManager;

    public BotSyncWorker(BotManager botManager)
    {
        _botManager = botManager;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Recycle of bot fetching from db (if bot toggle on/off)
        while (!stoppingToken.IsCancellationRequested)
        {
            await _botManager.SyncBotsAsync();
            await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken);
        }
    }
}
