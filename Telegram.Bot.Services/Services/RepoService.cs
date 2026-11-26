namespace Telegram.Bot.Services.Services
{
    public class RepoService
    {
        // _httpClient isn't exposed publicly
        private readonly HttpClient _httpClient;

        public RepoService(HttpClient client)
        {
            _httpClient = client;
        }

        //public async Task<IEnumerable<string>> GetRepos()
        //{
        //    var response = await _httpClient.GetAsync("aspnet/repos");

        //    response.EnsureSuccessStatusCode();

        //    using var responseStream = await response.Content.ReadAsStreamAsync();
        //    return await JsonSerializer.DeserializeAsync
        //        <IEnumerable<string>>(responseStream);
        //}
    }
}
