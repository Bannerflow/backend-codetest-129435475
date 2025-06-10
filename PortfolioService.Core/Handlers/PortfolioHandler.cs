using MongoDB.Bson;
using PortfolioService.Infrastructure.Models;
using PortfolioService.Infrastructure.Persistence;
using StockService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PortfolioService.Core.Handlers
{
    public class PortfolioHandler
    {
        private readonly IDataService _dataService;
        private readonly CurrencyConversionHandler _currencyConversionHandler;
        private readonly IStockService _stockService;

        public PortfolioHandler(IDataService dataService, CurrencyConversionHandler currencyConversionHandler, IStockService stockService)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _currencyConversionHandler = currencyConversionHandler ?? throw new ArgumentNullException(nameof(currencyConversionHandler));
            _stockService = stockService;
        }

        public async Task<PortfolioData> GetPortfolioAsync(ObjectId id)
        {
            if (id == ObjectId.Empty) {
                throw new ArgumentException("Invalid portfolio ID.", nameof(id));
            }
            var portfolio = await _dataService.GetPortfolio(id);

            if (portfolio == null || portfolio.Deleted) {
                throw new KeyNotFoundException($"Portfolio with ID {id} not found.");
            }

            return portfolio;
        }

        public async Task<decimal> GetPortfolioValueAsync(ObjectId id, string currency = "USD")
        {
            if (id == ObjectId.Empty) {
                throw new ArgumentException("Invalid portfolio ID.", nameof(id));
            }

            var portfolio = await GetPortfolioAsync(id);

            decimal totalValue = 0;
            foreach (var stock in portfolio.Stocks) {
                var (tickerPrice, tickerCurrency) = await _stockService.GetCurrentStockPrice(stock.Ticker);
                var stockPrice = await _currencyConversionHandler.ConvertAmount(tickerPrice, stock.BaseCurrency, currency); // Convert from tickerCurrency to stock baseCurrency then to currency?????????
                totalValue += stockPrice * stock.NumberOfShares;
            }
            return totalValue;
        }

        public async Task DeletePortfolioAsync(ObjectId id)
        {
            if (id == ObjectId.Empty) {
                throw new ArgumentException("Invalid portfolio ID.", nameof(id));
            }
            await _dataService.DeletePortfolio(id);
        }
    }
}
