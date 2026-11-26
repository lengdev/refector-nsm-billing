using PuppeteerSharp;
using PuppeteerSharp.Media;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace NSMSOLUTION.Services
{
    public class PuppeteerPdfService
    {
        private static readonly SemaphoreSlim BrowserDownloadLock = new SemaphoreSlim(1, 1);
        private static string _browserExecutablePath;

        private static async Task<string> EnsureChromiumAsync()
        {
            if (!string.IsNullOrWhiteSpace(_browserExecutablePath) && File.Exists(_browserExecutablePath))
            {
                return _browserExecutablePath;
            }

            await BrowserDownloadLock.WaitAsync();
            try
            {
                if (!string.IsNullOrWhiteSpace(_browserExecutablePath) && File.Exists(_browserExecutablePath))
                {
                    return _browserExecutablePath;
                }

                var customBrowserPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "chromium-cache");

                if (!Directory.Exists(customBrowserPath))
                {
                    Directory.CreateDirectory(customBrowserPath);
                }

                var browserFetcher = new BrowserFetcher(new BrowserFetcherOptions
                {
                    Path = customBrowserPath
                });

                var installedBrowser = await browserFetcher.DownloadAsync();
                _browserExecutablePath = installedBrowser.GetExecutablePath();

                return _browserExecutablePath;
            }
            finally
            {
                BrowserDownloadLock.Release();
            }
        }

        public async Task<byte[]> GeneratePdfAsync(string html, string baseUrl = null)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                throw new ArgumentException("HTML cannot be empty.", nameof(html));
            }

            var executablePath = await EnsureChromiumAsync();

            var launchOptions = new LaunchOptions
            {
                Headless = true,
                ExecutablePath = executablePath,
                Args = new[]
                {
            "--no-sandbox",
            "--disable-setuid-sandbox",
            "--disable-dev-shm-usage",
            "--disable-gpu",
            "--font-render-hinting=medium"
        }
            };

            using (var browser = await Puppeteer.LaunchAsync(launchOptions))
            {
                using (var page = await browser.NewPageAsync())
                {
                    if (!string.IsNullOrWhiteSpace(baseUrl))
                    {
                        html = InjectBaseUrl(html, baseUrl);
                    }

                    await page.SetContentAsync(html, new SetContentOptions
                    {
                        WaitUntil = new[]
                        {
                    WaitUntilNavigation.Load
                },
                        Timeout = 30000
                    });

                    await page.EvaluateExpressionAsync("document.fonts ? document.fonts.ready : Promise.resolve()");

                    var pdfOptions = new PdfOptions
                    {
                        Format = PaperFormat.A4,
                        Landscape = false,
                        PrintBackground = true,
                        //MarginOptions = new MarginOptions
                        //{
                        //    Top = "10mm",
                        //    Bottom = "15mm", // Extra space at bottom so footer doesn't overlap text
                        //    Left = "10mm",
                        //    Right = "10mm"
                        //},
                        DisplayHeaderFooter = true,
                        HeaderTemplate = "<span></span>", // Hides the default browser title/date header
                        FooterTemplate = @"
                    <div style='width:100%; font-size:8px; text-align:center; color:#555; font-family:Arial,sans-serif;'>
                        Page <span class='pageNumber'></span> of <span class='totalPages'></span>
                    </div>"
                    };

                    using (var pdfStream = await page.PdfStreamAsync(pdfOptions))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            await pdfStream.CopyToAsync(memoryStream);
                            return memoryStream.ToArray();
                        }
                    }
                }
            }
        }

        private static string InjectBaseUrl(string html, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                return html;

            var baseTag = "<base href=\"" + baseUrl.TrimEnd('/') + "/\" />";

            return html.IndexOf("<head>", StringComparison.OrdinalIgnoreCase) >= 0
                ? html.Replace("<head>", "<head>" + baseTag)
                : "<html><head>" + baseTag + "</head><body>" + html + "</body></html>";
        }
    }
}