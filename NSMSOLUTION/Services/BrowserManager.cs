using PuppeteerSharp;
using System.Threading.Tasks;

namespace NSMSOLUTION.Services
{
    public static class BrowserManager
    {
        private static Browser _browser;
        private static readonly object _lock = new object();

        public static async Task<Browser> GetBrowserAsync()
        {
            if (_browser != null)
                return _browser;

            lock (_lock)
            {
                if (_browser != null)
                    return _browser;
            }

            var fetcher = new BrowserFetcher();
            await fetcher.DownloadAsync();

            _browser = (Browser)await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true,
                Args = new[]
                {
                "--no-sandbox",
                "--disable-gpu"
            }
            });

            return _browser;
        }
    }
}