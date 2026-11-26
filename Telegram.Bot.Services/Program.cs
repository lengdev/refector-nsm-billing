using Telegram.Bot.Services;
using Telegram.Bot.Services.Models;
using Telegram.Bot.Services.Services;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args)
        .ConfigureServices((context, services) =>
        {
            services.AddHttpClient<HttpClientService>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(5);
            });

            services.AddScoped<UpdateHandler>();
            services.AddScoped<ReceiverService>();
            services.AddSingleton<BotManager>();
            services.AddSingleton<TelegramConfig>();

            services.AddHostedService<BotSyncWorker>();
            services.AddHostedService<TimerService>();
        });

        var host = builder.Build();

        // Initialize TelegramConfig.BotClient BEFORE anything else runs,
        // so every service can access _telegramConfig.BotClient synchronously.
        var telegramConfig = host.Services.GetRequiredService<TelegramConfig>();
        await telegramConfig.InitializeAsync();

        await host.RunAsync();
    }
}