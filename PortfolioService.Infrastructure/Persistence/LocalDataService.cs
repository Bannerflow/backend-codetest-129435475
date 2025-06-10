using System;
using Mongo2Go;
using MongoDB.Driver;
using PortfolioService.Infrastructure.Integrations;
using PortfolioService.Infrastructure.Models;

namespace PortfolioService.Infrastructure.Persistence
{
    public class LocalDataService : DataService, IDisposable
    {
        private readonly MongoDbRunner _runner = MongoDbRunner.Start();

        public LocalDataService(DataServiceSettings settings)
        {
            if (settings == null) {
                throw new ArgumentNullException(nameof(settings));
            }
            if (string.IsNullOrEmpty(settings.ConnectionString) && settings.LocalDb == null) {
                throw new ArgumentException("Local database settings must be provided.", nameof(settings));
            }

            foreach((var collectionName, var fileUrl) in settings.LocalDb!.Collections) {
                _runner.Import("portfolioServiceDb", collectionName, fileUrl, true);
            }

            var client = new MongoClient(_runner.ConnectionString);
            this._portfolioCollection = client.GetDatabase("portfolioServiceDb").GetCollection<PortfolioData>("Portfolios");
        }

        public void Dispose()
        {
            _runner?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}