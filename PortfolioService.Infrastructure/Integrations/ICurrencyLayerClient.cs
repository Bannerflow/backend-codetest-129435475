using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PortfolioService.Infrastructure.Integrations
{
    public interface ICurrencyLayerClient
    {
        public Task<Dictionary<string, decimal>> GetConversionRatesAsync();
    }
}
