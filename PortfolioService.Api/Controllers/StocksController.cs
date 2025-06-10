using System;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Threading.Tasks;
using PortfolioService.Core.Handlers;

namespace PortfolioService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PortfolioController : ControllerBase
    {
        private readonly PortfolioHandler _portfolioHandler;

        public PortfolioController(PortfolioHandler portfolioHandler)
        {
            _portfolioHandler = portfolioHandler ?? throw new ArgumentNullException(nameof(portfolioHandler));
        }

        /// <summary>
        /// Gets the portfolio details by ID.
        /// </summary>
        /// <param name="id">The ID of the portfolio</param>
        /// <returns>Portfolio details</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) {
                return BadRequest("Invalid portfolio ID format.");
            }
            var portfolio = await _portfolioHandler.GetPortfolioAsync(objectId);
            return Ok(portfolio);
        }

        /// <summary>
        /// Gets the combined value of all stocks in the portfolio in the specified currency.
        /// </summary>
        /// <param name="id">The ID of the portfolio</param>
        /// <param name="currency"> The currency to convert the total value into (default is USD)</param>
        /// <returns>Total value of the portfolio</returns>
        [HttpGet("{id}/value")]
        public async Task<IActionResult> GetTotalPortfolioValue(string id, string currency = "USD")
        {
            if (!ObjectId.TryParse(id, out var objectId)) {
                return BadRequest("Invalid portfolio ID format.");
            }
            var totalAmount = await _portfolioHandler.GetPortfolioValueAsync(objectId, currency);
            return Ok(totalAmount);
        }

        /// <summary>
        /// Soft deletes the portfolio by ID.
        /// </summary>
        /// <param name="id">The ID of the portfolio</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePortfolio(string id)
        {
            if (!ObjectId.TryParse(id, out var objectId)) {
                return BadRequest("Invalid portfolio ID format.");
            }
            await _portfolioHandler.DeletePortfolioAsync(objectId);
            return Ok();
        }
    }
}