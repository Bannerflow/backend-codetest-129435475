using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortfolioService.Infrastructure.Integrations
{
    public class CurrencyLayerSettings
    {
        public string ApiKey { get; set; }
        public string BaseUrl { get; set; } = "http://api.currencylayer.com/";
        public string DefaultCurrency { get; set; } = "USD";
        public List<string> SupportedCurrencies { get; set; } = new List<string>
        {
            "USD", "SEK", "NOK", "CAD", "EUR"
        };
    }

    class Response
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
        [JsonPropertyName("terms")]
        public string Terms { get; set; }
        [JsonPropertyName("privacy")]
        public string Privacy { get; set; }
        [JsonPropertyName("timestamp")]
        public int Timestamp { get; set; }
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("quotes")]
        public Dictionary<string, decimal> Quotes { get; set; }
    }

    public class CurrencyLayerClient : ICurrencyLayerClient
    {
        private readonly CurrencyLayerSettings _settings;
        public CurrencyLayerClient(CurrencyLayerSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrEmpty(_settings.ApiKey)) {
                throw new ArgumentException("API key must be provided.", nameof(settings));
            }
        }
        
        public async Task<Dictionary<string, decimal>> GetConversionRatesAsync()
        {
            var baseCurrency = _settings.DefaultCurrency.ToUpperInvariant();

            using var httpClient = new HttpClient { BaseAddress = new Uri(_settings.BaseUrl) };
            var response = await httpClient.GetAsync($"live?access_key={_settings.ApiKey}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var data = System.Text.Json.JsonSerializer.Deserialize<Response>(content);
            if (data == null || !data.Success) {
                throw new Exception("Failed to retrieve conversion rates.");
            }
            var conversionRates = new Dictionary<string, decimal>();

            foreach (var currency in _settings.SupportedCurrencies) {
                if (currency == baseCurrency) {
                    conversionRates[currency] = 1m; // Base currency rate is always 1
                    continue;
                }
                if (data.Quotes.TryGetValue($"{baseCurrency}{currency}", out var rate)) {
                    conversionRates[currency] = rate;
                } else {
                    throw new Exception($"Conversion rate for {baseCurrency}{currency} not found.");
                }
            }

            return conversionRates;
        }
    }
}
