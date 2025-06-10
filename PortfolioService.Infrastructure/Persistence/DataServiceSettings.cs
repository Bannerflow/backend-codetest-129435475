using System.Collections.Generic;

namespace PortfolioService.Infrastructure.Persistence
{
    public class DataServiceSettings
    {
        public string ConnectionString { get; init; }
        public LocalDb LocalDb { get; init; }
    }

    public class LocalDb
    {
        public Dictionary<string, string> Collections { get; init; }
    }
}
