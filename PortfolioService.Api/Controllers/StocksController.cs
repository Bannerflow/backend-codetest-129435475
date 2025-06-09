using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using CodeTest.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Text.Json;

namespace CodeTest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly DataService _dataService;

        private class Quote
        {
            public bool success { get; set; }
            public string terms { get; set; }
            public string privacy { get; set; }
            public int timestamp { get; set; }
            public string source { get; set; }
            public Dictionary<string, decimal> quotes { get; set; }
        }

        public PortfolioController()
        {
            _dataService = new DataService();
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var portfolio = _dataService.GetPortfolio(ObjectId.Parse(id)).Result;
            return Ok(portfolio);
        }

        [HttpGet("/value")]
        public IActionResult GetTotalPortfolioValue(string portfolioId, string currency = "USD")
        {
            var portfolio = _dataService.GetPortfolio(ObjectId.Parse(portfolioId)).Result;
            var totalAmount = 0m;
            var stockService = new StockService.StockService();
            var apiAccessKey = "edcbcd5977de259ca7fb25077ca8a0f6";
            using (var httpClient = new HttpClient {BaseAddress = new Uri("http://api.currencylayer.com/")})
            {
                // See https://currencylayer.com/documentation for details about the api
                var foo = httpClient.GetAsync($"live?access_key={apiAccessKey}").Result;
                var data = JsonSerializer.DeserializeAsync<Quote>(foo.Content.ReadAsStream()).Result;

                foreach (var stock in portfolio.Stocks)
                {
                    if (stock.BaseCurrency == currency)
                    {
                        totalAmount += stockService.GetCurrentStockPrice(stock.Ticker).Result.Price *
                                       stock.NumberOfShares;
                    }
                    else
                    {
                        if (currency == "USD")
                        {
                            var stockPrice = stockService.GetCurrentStockPrice(stock.Ticker).Result.Price;
                            var rateUsd = data.quotes["USD" + stock.BaseCurrency];
                            totalAmount += stockPrice /rateUsd * stock.NumberOfShares;
                        }
                        else
                        {
                            var stockPrice = stockService.GetCurrentStockPrice(stock.Ticker).Result.Price;
                            var rateUsd = data.quotes["USD" + stock.BaseCurrency];
                            var amount = stockPrice / rateUsd * stock.NumberOfShares;
                            var targetRateUsd = data.quotes["USD" + currency];
                            totalAmount += amount * targetRateUsd;
                        }
                    }
                }
            }

            return Ok(totalAmount);
        }

        [HttpGet("/delete")]
        public IActionResult DeletePortfolio(string portfolioId)
        {
            var dataService = new DataService();
            dataService.DeletePortfolio(ObjectId.Parse(portfolioId));
            return Ok();
        }
    }
}