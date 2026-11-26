using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Services;
using Telegram.Bot.Types.Enums;

public class PollingService : BackgroundService
{
    private readonly ILogger<PollingService> _logger;
    private readonly IServiceProvider _services;
    private readonly ITelegramBotClient _botClient;
    private readonly UpdateHandler _updateHandler;
    private readonly string _botName;

    public PollingService(
        ILogger<PollingService> logger,
        IServiceProvider services,
        ITelegramBotClient botClient,
        UpdateHandler updateHandler,
        string botName)
    {
        _logger = logger;
        _services = services;
        _botClient = botClient;
        _updateHandler = updateHandler;
        _botName = botName;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"[{_botName}] Starting polling...");

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        _botClient.StartReceiving(
            updateHandler: _updateHandler,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken);

        var me = await _botClient.GetMe(cancellationToken: stoppingToken);
        _logger.LogInformation($"\u001b[32m[{_botName}] Bot {me.Username} is up and running.");
    }
}
