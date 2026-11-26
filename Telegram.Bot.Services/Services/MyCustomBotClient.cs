using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Abstract;
using Telegram.Bot.Polling;
using Telegram.Bot.Requests.Abstractions;

namespace Telegram.Bot.Services.Services
{
    public class MyCustomBotClient : TelegramBotClient
    {
        public int Timer = 0;
        public MyCustomBotClient(TelegramBotClientOptions options,
        HttpClient? httpClient = default)
            : base(options, httpClient)
        {
            // Set your desired timeout value (in milliseconds)
            //httpClient.Timeout = TimeSpan.FromSeconds(30); // Example: 30 seconds
        }

        // Override the MakeRequestAsync method to customize API requests
        public override async Task<TResponse> MakeRequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            // Your custom logic here
            // For example, you can log requests or modify them before sending
            Timer += 1;
            Console.WriteLine("Timer: " + Timer.ToString() + " Date Time: " + DateTime.Now);
            return await base.MakeRequestAsync(request, cancellationToken);
        }

        // Add any other custom methods or overrides you need
    }
}
