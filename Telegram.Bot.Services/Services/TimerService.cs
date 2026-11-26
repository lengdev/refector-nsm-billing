using Telegram.Bot.Services.Models;

namespace Telegram.Bot.Services;

/// <summary>
/// Runs a set of recurring background jobs on a single minute-resolution timer.
/// Add/remove jobs in StartAsync — everything else is generic.
/// </summary>
public class TimerService : IHostedService, IDisposable
{
    private readonly ILogger<TimerService> _logger;
    private readonly HttpClientService _httpClientService;
    private readonly string _apiBaseUrl;
    private readonly TelegramConfig _telegramConfig;
    private readonly List<ScheduledJob> _jobs = [];
    private Timer? _timer;

    public TimerService(
        ILogger<TimerService> logger,
        HttpClientService httpClientService,
        TelegramConfig telegramConfig)
    {
        _logger = logger;
        _telegramConfig = telegramConfig;
        _httpClientService = httpClientService;
        _apiBaseUrl = telegramConfig.GetSection("BASE_URL").Value ?? "";
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduler started");

        // Every 2 hours: a batch of quick reminder calls
        _jobs.Add(new ScheduledJob("DoWork", TimeSpan.FromHours(2), RunDoWorkBatch));

        _jobs.Add(new ScheduledJob("StockAlert", TimeSpan.FromDays(1),
            () => CallApi("StockAlert"), NextRunAt(9, 0)));

        _jobs.Add(new ScheduledJob("StockExpireAlert", TimeSpan.FromDays(1),
            () => CallApi("StockExpireAlert"), NextRunAt(9, 0)));

        _jobs.Add(new ScheduledJob("CustomerIdentificationExpire", TimeSpan.FromDays(1),
            () => CallApi("Customer/Identification", "CustomerIdentificationExpire"), NextRunAt(9, 0)));

        // Weekly jobs (Monday only)
        _jobs.Add(new ScheduledJob("ExpiredPDF", TimeSpan.FromDays(1),
            () => CallApi("ExpiredPDF"), NextRunAt(9, 0), onlyOnDay: DayOfWeek.Monday));

        _jobs.Add(new ScheduledJob("ReorderPDF", TimeSpan.FromDays(1),
            () => CallApi("ReorderPDF"), NextRunAt(9, 0), onlyOnDay: DayOfWeek.Monday));

        // Disabled: ReminderPI every 15 minutes
        // _jobs.Add(new ScheduledJob("ReminderPI", TimeSpan.FromMinutes(15), () => CallApi("ReminderPI")));

        _timer = new Timer(OnTick, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    // ReminderAdvExpense removed from here — it now runs as its own
    // ScheduledJob above, with a longer timeout.
    private Task RunDoWorkBatch() =>
        Task.WhenAll(
            CallApi("GetExchangeRate"),
            CallApi("ReminderRequestForm"),
            CallApi("ReminderInventoryRequest"),
            CallApi("ReminderAdvExpense"),
            CallApi("ReminderPO"));

    private async Task CallApi(string endpoint, string? name = null, TimeSpan? timeout = null)
    {
        name ??= endpoint;
        var url = $"{_apiBaseUrl}/api/{endpoint}";
        try
        {
            var response = await _httpClientService.GetAsync(url, timeout);
            _logger.LogInformation("{Job} response: {Status}", name, response.StatusCode);
        }
        catch (TimeoutException ex)
        {
            _logger.LogError(ex, "Timeout calling {Job}", name);
            await _telegramConfig.ReportErrorAsync($"Timeout calling job '{name}'", ex, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling {Job}", name);
            await _telegramConfig.ReportErrorAsync($"Error calling job '{name}'", ex, CancellationToken.None);
        }
    }

    private void OnTick(object? state) => _ = RunDueJobsAsync();

    private async Task RunDueJobsAsync()
    {
        var now = DateTime.Now;
        foreach (var job in _jobs.Where(j => j.ShouldRun(now)))
        {
            try
            {
                _logger.LogInformation("Running job {Job}", job.Name);
                await job.Action();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running job {Job}", job.Name);
                await _telegramConfig.ReportErrorAsync($"Unhandled exception running job '{job.Name}'", ex, CancellationToken.None);
            }
            finally
            {
                // Always reschedule, even on failure, so a broken job
                // doesn't fire on every tick forever.
                job.ScheduleNext(now);
            }
        }
    }

    private static DateTime NextRunAt(int hour, int minute)
    {
        var next = DateTime.Today.AddHours(hour).AddMinutes(minute);
        return next <= DateTime.Now ? next.AddDays(1) : next;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// A single recurring job. Optionally restricted to one day of the week.
/// </summary>
public class ScheduledJob
{
    public string Name { get; }
    public Func<Task> Action { get; }
    public DateTime NextRun { get; private set; }

    private readonly TimeSpan _interval;
    private readonly DayOfWeek? _onlyOnDay;

    public ScheduledJob(string name, TimeSpan interval, Func<Task> action,
        DateTime? firstRun = null, DayOfWeek? onlyOnDay = null)
    {
        Name = name;
        _interval = interval;
        Action = action;
        _onlyOnDay = onlyOnDay;
        NextRun = firstRun ?? DateTime.Now;
    }

    public bool ShouldRun(DateTime now) =>
        now >= NextRun && (_onlyOnDay is null || now.DayOfWeek == _onlyOnDay);

    public void ScheduleNext(DateTime now)
    {
        var next = NextRun.Add(_interval);
        while (next <= now || (_onlyOnDay is not null && next.DayOfWeek != _onlyOnDay))
            next = next.Add(_interval);

        NextRun = next;
    }
}