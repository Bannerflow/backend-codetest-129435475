using MongoDB.Bson;
using PortfolioService.Infrastructure.Models;
using System.Threading.Tasks;

namespace PortfolioService.Infrastructure.Persistence
{
    public interface IDataService
    {
        public Task<PortfolioData> GetPortfolio(ObjectId id);
        public Task DeletePortfolio(ObjectId id);
    }
}
