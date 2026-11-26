using System.Net.Http.Headers;
using System.Text;

namespace Telegram.Bot.Services.Models
{
    public class HttpClientService
    {
        private readonly HttpClient _client;
        private readonly TelegramConfig _telegramConfig;
        private readonly string _apiKey;
        private readonly string _authUser;
        private readonly string _authPwd;

        public HttpClientService(HttpClient client, TelegramConfig telegramConfig)
        {
            _telegramConfig = telegramConfig;
            _apiKey = _telegramConfig.GetSection("API_KEY").Value ?? "";
            _authUser = _telegramConfig.GetSection("AUTH_USER").Value ?? "";
            _authPwd = _telegramConfig.GetSection("AUTH_PWD").Value ?? "";

            _client = client;
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_authUser}:{_authPwd}"));

            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", base64);
        }

        /// <summary>
        /// GET with an optional per-call timeout. If omitted, falls back to
        /// whatever HttpClient.Timeout is configured at registration (default 100s).
        /// </summary>
        public async Task<HttpResponseMessage> GetAsync(string url, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
        {
            if (timeout is null)
            {
                return await _client.GetAsync(url, cancellationToken);
            }

            // Combine the caller's token with a per-call timeout, without
            // touching HttpClient.Timeout (which can only be set once, pre-first-request).
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout.Value);

            try
            {
                return await _client.GetAsync(url, cts.Token);
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // The linked token fired because of OUR timeout, not the caller's token —
                // rethrow as a clearer, purpose-specific exception.
                throw new TimeoutException($"Request to {url} timed out after {timeout.Value}.");
            }
        }
    }
}