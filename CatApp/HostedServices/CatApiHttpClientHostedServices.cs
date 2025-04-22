using CatApp.Shared.Options;
using Microsoft.Extensions.Options;

namespace CatApp.HostedServices
{
    /// <summary>
    /// Registering the desired configuration setting for our super mew http 
    /// </summary>
    public class CatApiHttpClientHostedService : IHostedService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly HttpClientOptions _options;
        private readonly ILogger<CatApiHttpClientHostedService> _logger;

        public CatApiHttpClientHostedService(
            IHttpClientFactory httpClientFactory,
            IOptions<HttpClientOptions> options,
            ILogger<CatApiHttpClientHostedService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var client = _httpClientFactory.CreateClient("CatApi");

            // Set base address and custom headers from configuration
            client.BaseAddress = new Uri(_options.ServerUrl);

            foreach (var header in _options.HeadersAndValues)
            {
                if (!client.DefaultRequestHeaders.Contains(header.Key))
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }

            _logger.LogInformation("Initializing our super http clinet's  - MEW - base URL: {Url}", _options.ServerUrl);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cat API HttpClient hosted service stopping.");
            return Task.CompletedTask;
        }
    }
}
