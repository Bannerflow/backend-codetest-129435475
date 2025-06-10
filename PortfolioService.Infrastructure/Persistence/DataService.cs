using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using PortfolioService.Infrastructure.Models;

namespace PortfolioService.Infrastructure.Persistence
{
    public class DataService : IDataService
    {
        protected IMongoCollection<PortfolioData> _portfolioCollection;

        public DataService(IMongoCollection<PortfolioData> portfolioCollection) 
        {
            _portfolioCollection = portfolioCollection ?? throw new ArgumentNullException(nameof(portfolioCollection));
        }

        public DataService()
        {
            // Default constructor for dependency injection or testing purposes
        }

        public async Task<PortfolioData> GetPortfolio(ObjectId id)
        {
            var idFilter = Builders<PortfolioData>.Filter.Eq(portfolio => portfolio.Id, id);

            return await _portfolioCollection.Find(idFilter).FirstOrDefaultAsync();
        }

        public async Task DeletePortfolio(ObjectId id)
        {
            await _portfolioCollection.UpdateOneAsync(
                Builders<PortfolioData>.Filter.Eq(portfolio => portfolio.Id, id),
                Builders<PortfolioData>.Update.Set(p => p.Deleted, true)
            );
        }
    }
}