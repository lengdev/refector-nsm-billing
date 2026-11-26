using Microsoft.Data.SqlClient;
using Telegram.Bot;

public class BotInfo
{
    public string BotName { get; set; } = "";
    public string Token { get; set; } = "";
}

public class TelegramConfig
{
    private readonly string _connectionString;
    private readonly ILogger<TelegramConfig> _logger;
    private readonly IConfiguration _configuration;
    private const long ErrorReportChatId = -4510188155;

    /// <summary>
    /// Shared bot client for cross-cutting concerns (error reporting, etc.)
    /// where "which bot" doesn't matter. Populated once via InitializeAsync()
    /// at startup — access it directly afterward, no await needed.
    /// </summary>
    public ITelegramBotClient? BotClient { get; private set; }

    public TelegramConfig(
        IConfiguration configuration,
        ILogger<TelegramConfig> logger
    )
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("AuthContextNSM")
            ?? throw new InvalidOperationException("Connection string 'AuthContextNSM' not found in configuration.");
        _logger = logger;
    }

    public SqlConnection GetConnection()
    {
        try
        {
            return new SqlConnection(_connectionString);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to create SqlConnection.", ex);
        }
    }

    public IConfigurationSection GetSection(string key)
    {
        return _configuration.GetSection(key);
    }

    public async Task<List<BotInfo>> LoadBotsFromDbAsync()
    {
        var bots = new List<BotInfo>();

        try
        {
            await using var conn = GetConnection();
            await conn.OpenAsync();

            const string sql = @"SELECT BotName, BotToken FROM dbo.TblTelegramBots WHERE IsActive = 1";
            await using var cmd = new SqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var botName = reader["BotName"] as string ?? string.Empty;
                var token = reader["BotToken"] as string ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(botName) && !string.IsNullOrWhiteSpace(token))
                {
                    bots.Add(new BotInfo { BotName = botName, Token = token });
                }
            }

            if (bots.Count > 0)
            {
                _logger.LogInformation(
                    "Loaded {Count} bots from database at {Time}.",
                    bots.Count,
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                return bots;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading bots from database — returning null to signal 'skip this sync cycle', not falling back to config.");
        }

        // Fallback to config if DB loading failed or no bots found
        bots = GetSection("BotConfiguration:Bots").Get<List<BotInfo>>() ?? [];
        _logger.LogWarning("No active bots found in database — falling back to configuration.");
        return bots;
    }

    /// <summary>
    /// Loads bots and builds the shared BotClient. Call this once at
    /// application startup, before any service tries to use BotClient.
    /// </summary>
    public async Task InitializeAsync()
    {
        var bots = await LoadBotsFromDbAsync();
        var firstBot = bots.FirstOrDefault();

        if (firstBot is null || string.IsNullOrWhiteSpace(firstBot.Token))
        {
            _logger.LogError("No bot token available — TelegramConfig.BotClient will be null.");
            return;
        }

        BotClient = new TelegramBotClient(firstBot.Token);
        _logger.LogInformation("TelegramConfig.BotClient initialized using bot '{BotName}'.", firstBot.BotName);
    }

    public async Task ReportErrorAsync(string context, Exception ex, CancellationToken cancellationToken)
    {
        try
        {
            if (BotClient is null)
            {
                _logger.LogError("Cannot report error to Telegram — BotClient not initialized. Context: {Context}", context);
                return;
            }

            var text =
                $"🔴 <b>Bot Error</b>\n" +
                $"<b>Context:</b> {System.Net.WebUtility.HtmlEncode(context)}\n" +
                $"<b>Message:</b> {System.Net.WebUtility.HtmlEncode(ex.Message)}\n" +
                $"<b>Time:</b> {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            await BotClient.SendMessage(
                chatId: ErrorReportChatId,
                text: text,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Html,
                cancellationToken: cancellationToken
            );
        }
        catch (Exception reportEx)
        {
            // Never let error-reporting itself throw and mask the original error
            _logger.LogError(reportEx, "Failed to send error report to group {ChatId}", ErrorReportChatId);
        }
    }
}