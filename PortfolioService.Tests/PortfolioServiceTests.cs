using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mongo2Go;
using MongoDB.Driver;
using Moq;
using PortfolioService.Core.Handlers;
using PortfolioService.Infrastructure.Integrations;
using PortfolioService.Infrastructure.Models;
using PortfolioService.Infrastructure.Persistence;
using StockService;

namespace PortfolioService.Test
{
    [TestClass]
    public class PortfolioServiceTests
    {
        [TestMethod]
        public async Task TestRetrievalOfPortfolio()
        {
            using var _runner = MongoDbRunner.Start();
            var collection = new MongoClient(_runner.ConnectionString)
                .GetDatabase("portfolioServiceDb")
                .GetCollection<PortfolioData>("Portfolios");

            var objectId = MongoDB.Bson.ObjectId.GenerateNewId();
            var mockData = new PortfolioData {
                Id = objectId,
                Deleted = false,
                Stocks = new List<StockData>
                {
                    new StockData { Ticker = "AAPL", BaseCurrency = "USD", NumberOfShares = 10 },
                    new StockData { Ticker = "GOOGL", BaseCurrency = "USD", NumberOfShares = 5 }
                }
            };
            await collection.InsertOneAsync(mockData);
            var dataService = new DataService(collection);
            var currencyLayerClientMock = new Mock<ICurrencyLayerClient>();
            //currencyLayerClientMock.Setup(client => client.GetConversionRatesAsync())
            //   .ReturnsAsync(new Dictionary<string, decimal> { });
            var portfolioHandler = new PortfolioHandler(dataService, new CurrencyConversionHandler(currencyLayerClientMock.Object), new StockService.StockService());

            var portfolio = await portfolioHandler.GetPortfolioAsync(objectId);
            Assert.IsNotNull(portfolio);
        }

        [TestMethod]
        public async Task TestDeletionOfPortfolio()
        {
            using var _runner = MongoDbRunner.Start();
            var collection = new MongoClient(_runner.ConnectionString)
                .GetDatabase("portfolioServiceDb")
                .GetCollection<PortfolioData>("Portfolios");
            var objectId = MongoDB.Bson.ObjectId.GenerateNewId();
            var mockData = new PortfolioData {
                Id = objectId,
                Deleted = false,
                Stocks = new List<StockData>
                {
                    new StockData { Ticker = "AAPL", BaseCurrency = "USD", NumberOfShares = 10 },
                    new StockData { Ticker = "GOOGL", BaseCurrency = "USD", NumberOfShares = 5 }
                }
            };
            await collection.InsertOneAsync(mockData);
            var dataService = new DataService(collection);
            var currencyLayerClientMock = new Mock<ICurrencyLayerClient>();
            //currencyLayerClientMock.Setup(client => client.GetConversionRatesAsync())
            //   .ReturnsAsync(new Dictionary<string, decimal> { });
            var portfolioHandler = new PortfolioHandler(dataService, new CurrencyConversionHandler(currencyLayerClientMock.Object), new StockService.StockService());

            var portfolioBeforeDeletion = await portfolioHandler.GetPortfolioAsync(objectId);
            Assert.IsNotNull(portfolioBeforeDeletion);
            Assert.IsFalse(portfolioBeforeDeletion.Deleted);

            await portfolioHandler.DeletePortfolioAsync(objectId);

            var deletedPortfolio = await dataService.GetPortfolio(objectId);
            Assert.IsTrue(deletedPortfolio.Deleted);

            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(async () => {
                await portfolioHandler.GetPortfolioAsync(objectId);
            });
        }

        [TestMethod]
        public async Task TestGetPortfolioValue()
        {
            using var _runner = MongoDbRunner.Start();
            var collection = new MongoClient(_runner.ConnectionString)
                .GetDatabase("portfolioServiceDb")
                .GetCollection<PortfolioData>("Portfolios");
            var objectId = MongoDB.Bson.ObjectId.GenerateNewId();
            var mockData = new PortfolioData {
                Id = objectId,
                Deleted = false,
                Stocks = new List<StockData>
                {
                    new StockData { Ticker = "AAPL", BaseCurrency = "USD", NumberOfShares = 10 },
                    new StockData { Ticker = "VOLV B", BaseCurrency = "SEK", NumberOfShares = 5 }
                }
            };
            await collection.InsertOneAsync(mockData);
            var dataService = new DataService(collection);
            var currencyLayerClientMock = new Mock<ICurrencyLayerClient>();

            currencyLayerClientMock.Setup(client => client.GetConversionRatesAsync())
                .ReturnsAsync(new Dictionary<string, decimal> {
                    {"USD", 1m },
                    { "SEK", 10m }
                });

            // Mocking the StockService to return fixed prices for the stocks
            var stockServiceMock = new Mock<IStockService>();
            stockServiceMock.Setup(service => service.GetCurrentStockPrice("AAPL"))
                .ReturnsAsync((100m, "USD")); // AAPL price in USD
            stockServiceMock.Setup(service => service.GetCurrentStockPrice("VOLV B"))
                .ReturnsAsync((50m, "SEK")); // VOLV B price in SEK

            var portfolioHandler = new PortfolioHandler(dataService, new CurrencyConversionHandler(currencyLayerClientMock.Object), stockServiceMock.Object);
            var totalValueUSD = await portfolioHandler.GetPortfolioValueAsync(objectId, "USD");
            Assert.AreEqual(1025, totalValueUSD); // 10 apple at 100 = 1000 + 5 volvo at 50 sek => 5 usd = 25 = 1025

            var totalValueSEK = await portfolioHandler.GetPortfolioValueAsync(objectId, "SEK");
            Assert.AreEqual(10250, totalValueSEK); // 10 apple at 100 usd => 1000 sek = 10000 + 5 volvo at 50 sek = 250 = 10250
        }
    }
}
