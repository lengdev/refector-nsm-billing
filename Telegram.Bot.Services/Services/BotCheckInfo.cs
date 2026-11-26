using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class BotCheckInfo : BackgroundService
{
    private readonly ILogger<BotCheckInfo> _logger;
    private readonly ITelegramBotClient _botClient;

    public BotCheckInfo(ILogger<BotCheckInfo> logger, ITelegramBotClient botClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandleErrorAsync,
            new ReceiverOptions { AllowedUpdates = { } },
            cancellationToken: stoppingToken
        );

        var me = await _botClient.GetMe();
        _logger.LogInformation("Bot started: @{Username}", me.Username);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        if (update.Type == UpdateType.Message && update.Message?.Text != null)
        {
            var msg = update.Message;

            if (msg.Text.StartsWith("/check"))
            {
                await SendGroupInfo(bot, msg);
            }
        }
    }

    private Task HandleErrorAsync(ITelegramBotClient bot, Exception ex, CancellationToken token)
    {
        _logger.LogError(ex, "Telegram Bot Error");
        return Task.CompletedTask;
    }

    private async Task SendGroupInfo(ITelegramBotClient bot, Telegram.Bot.Types.Message message)
    {
        var chat = message.Chat;
        var chatIdStr = chat.Id.ToString().Replace("-", "\\-");

        string info =
            $@"`📂 Group Info`
`|- id:` ||{chatIdStr}||
`|- type: {chat.Type}`
`|- title: {chat.Title}`";

        try
        {
            int membersCount = await bot.GetChatMemberCount(chat.Id);
            info += $"\n`|- members_count: {membersCount}`";
        }
        catch (Exception e)
        {
            info += $"\n`|- members_count: error ({e.Message})`";
        }

        await bot.SendMessage(
            chatId: chat.Id,
            text: info,
            parseMode: ParseMode.MarkdownV2
        );
    }
}