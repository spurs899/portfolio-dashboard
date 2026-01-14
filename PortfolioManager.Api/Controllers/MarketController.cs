using Microsoft.AspNetCore.Mvc;
using PortfolioManager.Contracts.Models.Market;
using PortfolioManager.Core.Services.Market;

namespace PortfolioManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketController : ControllerBase
{
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly IOfflineMarketStatusCalculator _offlineMarketStatusCalculator;
    private readonly ILogger<MarketController> _logger;

    public MarketController(
        IMarketDataProvider marketDataProvider,
        IOfflineMarketStatusCalculator offlineMarketStatusCalculator,
        ILogger<MarketController> logger)
    {
        _marketDataProvider = marketDataProvider;
        _offlineMarketStatusCalculator = offlineMarketStatusCalculator;
        _logger = logger;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetMarketStatus()
    {
        try
        {
            var polygonResponse = await _marketDataProvider.GetMarketStatusAsync();
            
            if (polygonResponse?.Exchanges?.Nyse != null)
            {
                return Ok(new MarketStatusResponse
                {
                    Market = polygonResponse.Market,
                    ServerTime = polygonResponse.ServerTime,
                    NyseStatus = polygonResponse.Exchanges.Nyse,
                    NasdaqStatus = polygonResponse.Exchanges.Nasdaq,
                    IsOpen = polygonResponse.Market == "open",
                    Source = "polygon"
                });
            }

            var calculatedStatus = _offlineMarketStatusCalculator.CalculateMarketStatus();
            return Ok(calculatedStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching market status");
            var calculatedStatus = _offlineMarketStatusCalculator.CalculateMarketStatus();
            return Ok(calculatedStatus);
        }
    }
}
