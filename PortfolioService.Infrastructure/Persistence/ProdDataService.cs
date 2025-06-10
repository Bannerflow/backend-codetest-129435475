using System;
using MongoDB.Driver;
using PortfolioService.Infrastructure.Integrations;
using PortfolioService.Infrastructure.Models;

namespace PortfolioService.Infrastructure.Persistence
{
    public class ProdDataService : DataService
    {
        public ProdDataService(DataServiceSettings settings)
        {
            if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            }
            if (string.IsNullOrEmpty(settings.ConnectionString)) {
                throw new ArgumentException("Connection string must be provided.", nameof(settings));
            }

            var client = new MongoClient(settings.ConnectionString);
            this._portfolioCollection = client.GetDatabase("portfolioServiceDb").GetCollection<PortfolioData>("Portfolios");
        }
    }
}