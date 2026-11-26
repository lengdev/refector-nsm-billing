namespace Telegram.Bot.Services.Services
{
    public class BotManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TelegramConfig _telegramConfig;
        private readonly Dictionary<string, PollingService> _runningBots = [];
        private readonly ILogger<BotManager> _logger;

        public BotManager(
            IServiceProvider serviceProvider,
            TelegramConfig telegramConfig,
            ILogger<BotManager> logger)
        {
            _serviceProvider = serviceProvider;
            _telegramConfig = telegramConfig;
            _logger = logger;
        }

        public async Task SyncBotsAsync()
        {
            var botsFromDb = await _telegramConfig.LoadBotsFromDbAsync();
            if (botsFromDb is null)
            {
                _logger.LogWarning("SyncBotsAsync: DB load failed transiently — skipping this cycle, leaving currently running bots untouched.");
                return; 
            }

            foreach (var bot in botsFromDb)
            {
                if (!_runningBots.ContainsKey(bot.Token)) 
                {
                    _logger.LogInformation("Starting bot: {BotName}", bot.BotName);
                    var pollingService = CreatePollingService(bot);
                    _runningBots[bot.Token] = pollingService;
                    await pollingService.StartAsync(CancellationToken.None);
                }
            }

            var activeTokens = botsFromDb.Select(b => b.Token).ToHashSet();
            foreach (var token in _runningBots.Keys.ToList())
            {
                if (!activeTokens.Contains(token))
                {
                    _logger.LogInformation("Stopping bot (token no longer active)");
                    await _runningBots[token].StopAsync(CancellationToken.None);
                    _runningBots.Remove(token);
                }
            }
        }

        private PollingService CreatePollingService(BotInfo bot)
        {
            var logger = _serviceProvider.GetRequiredService<ILogger<PollingService>>();
            var httpClientFactory = _serviceProvider.GetRequiredService<IHttpClientFactory>();
            var updateLogger = _serviceProvider.GetRequiredService<ILogger<UpdateHandler>>();
            var telegramConfig = _serviceProvider.GetRequiredService<TelegramConfig>();
            var httpClient = httpClientFactory.CreateClient();
            var botClient = new TelegramBotClient(bot.Token, httpClient);

            // Polling action 
            var updateHandler = new UpdateHandler(updateLogger, telegramConfig, botClient);

            return new PollingService(logger, _serviceProvider, botClient, updateHandler, bot.BotName);
        }
    }
}