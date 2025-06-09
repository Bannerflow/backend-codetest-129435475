using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockService
{
    public interface IStockService
    {
        Task<(decimal Price, string BaseCurrency)> GetCurrentStockPrice(string ticker);
    }

    public class StockService : IStockService
    {
        private readonly List<string> _baseCurrencies = new() { "USD", "SEK", "NOK", "CAD", "EUR" };

        public async Task<(decimal Price, string BaseCurrency)> GetCurrentStockPrice(string ticker)
        {
            var random = new Random();

            return await Task.FromResult((random.Next(20, 800), _baseCurrencies[random.Next(0, _baseCurrencies.Count)]));
        }
    }
}