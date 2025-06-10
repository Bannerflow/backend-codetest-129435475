using PortfolioService.Infrastructure.Integrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PortfolioService.Core.Handlers
{
    public class CurrencyConversionHandler
    {
        private ICurrencyLayerClient _currencyClient;
        private Dictionary<string, decimal> _conversionRates;
        private DateTime _lastUpdated;

        public CurrencyConversionHandler(ICurrencyLayerClient currencyLayerClient)
        {
            _currencyClient = currencyLayerClient ?? throw new ArgumentNullException(nameof(currencyLayerClient));
        }

        public async Task LoadConversionRates()
        {
            if (_conversionRates != null && (DateTime.Now - _lastUpdated).TotalDays < 1) {
                return;
            }
            _conversionRates = await _currencyClient.GetConversionRatesAsync();
            _lastUpdated = DateTime.Now;
        }

        private decimal CurrencyConversionRate(string currency)
        {
            if (_conversionRates == null || !_conversionRates.ContainsKey(currency)) {
                throw new KeyNotFoundException($"Conversion rate for {currency} not found.");
            }
            return _conversionRates[currency];
        }

        public async Task<decimal> ConvertAmount(decimal amount, string fromCurrency, string toCurrency)
        {
            if (string.IsNullOrEmpty(fromCurrency) || string.IsNullOrEmpty(toCurrency)) {
                throw new ArgumentException("Currency codes cannot be null or empty.");
            }

            await LoadConversionRates();

            var fromRate = CurrencyConversionRate(fromCurrency);
            var toRate = CurrencyConversionRate(toCurrency);

            return amount * (toRate / fromRate);
        }
    }
}
