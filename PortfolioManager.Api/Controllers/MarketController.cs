using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace PortfolioManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarketController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MarketController> _logger;

    public MarketController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<MarketController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetMarketStatus()
    {
        try
        {
            var apiKey = _configuration["Polygon:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogWarning("Polygon API key not configured");
                return Ok(GetFallbackMarketStatus());
            }

            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"https://api.polygon.io/v1/marketstatus/now?apiKey={apiKey}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning($"Polygon API returned {response.StatusCode}");
                return Ok(GetFallbackMarketStatus());
            }

            var polygonResponse = await response.Content.ReadFromJsonAsync<PolygonMarketStatusResponse>();
            
            if (polygonResponse?.Exchanges?.Nyse == null)
            {
                return Ok(GetFallbackMarketStatus());
            }

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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching market status from Polygon.io");
            return Ok(GetFallbackMarketStatus());
        }
    }

    private MarketStatusResponse GetFallbackMarketStatus()
    {
        var utcNow = DateTime.UtcNow;
        var isDST = IsDaylightSavingTime(utcNow);
        var easternOffset = isDST ? -4 : -5;
        var nyseNow = utcNow.AddHours(easternOffset);

        string status;
        bool isOpen;

        if (nyseNow.DayOfWeek == DayOfWeek.Saturday || nyseNow.DayOfWeek == DayOfWeek.Sunday)
        {
            status = "closed";
            isOpen = false;
        }
        else if (IsNyseHoliday(nyseNow))
        {
            status = "closed";
            isOpen = false;
        }
        else
        {
            var marketOpen = new TimeSpan(9, 30, 0);
            var marketClose = new TimeSpan(16, 0, 0);
            var currentTime = nyseNow.TimeOfDay;

            if (currentTime >= marketOpen && currentTime < marketClose)
            {
                status = "open";
                isOpen = true;
            }
            else if (currentTime < marketOpen)
            {
                status = "early_hours";
                isOpen = false;
            }
            else
            {
                status = "closed";
                isOpen = false;
            }
        }

        return new MarketStatusResponse
        {
            Market = status,
            ServerTime = nyseNow.ToString("yyyy-MM-ddTHH:mm:sszzz"),
            NyseStatus = status,
            NasdaqStatus = status,
            IsOpen = isOpen,
            Source = "calculated"
        };
    }

    private bool IsDaylightSavingTime(DateTime utcTime)
    {
        var year = utcTime.Year;

        // DST starts: 2nd Sunday in March at 2 AM
        var marchFirst = new DateTime(year, 3, 1);
        var daysUntilSunday = ((int)DayOfWeek.Sunday - (int)marchFirst.DayOfWeek + 7) % 7;
        var dstStart = marchFirst.AddDays(daysUntilSunday + 7).AddHours(7);

        // DST ends: 1st Sunday in November at 2 AM
        var novemberFirst = new DateTime(year, 11, 1);
        daysUntilSunday = ((int)DayOfWeek.Sunday - (int)novemberFirst.DayOfWeek + 7) % 7;
        var dstEnd = novemberFirst.AddDays(daysUntilSunday).AddHours(6);

        return utcTime >= dstStart && utcTime < dstEnd;
    }

    private bool IsNyseHoliday(DateTime date)
    {
        var year = date.Year;
        var holidays = GetNyseHolidays(year);
        return holidays.Any(h => h.Date == date.Date);
    }

    private List<DateTime> GetNyseHolidays(int year)
    {
        var holidays = new List<DateTime>();

        // New Year's Day (or observed)
        var newYears = new DateTime(year, 1, 1);
        holidays.Add(AdjustForWeekend(newYears));

        // Martin Luther King Jr. Day (3rd Monday in January)
        holidays.Add(GetNthWeekdayOfMonth(year, 1, DayOfWeek.Monday, 3));

        // Presidents' Day (3rd Monday in February)
        holidays.Add(GetNthWeekdayOfMonth(year, 2, DayOfWeek.Monday, 3));

        // Good Friday (Friday before Easter)
        holidays.Add(GetGoodFriday(year));

        // Memorial Day (Last Monday in May)
        holidays.Add(GetLastWeekdayOfMonth(year, 5, DayOfWeek.Monday));

        // Juneteenth (June 19, or observed)
        var juneteenth = new DateTime(year, 6, 19);
        holidays.Add(AdjustForWeekend(juneteenth));

        // Independence Day (July 4, or observed)
        var independenceDay = new DateTime(year, 7, 4);
        holidays.Add(AdjustForWeekend(independenceDay));

        // Labor Day (1st Monday in September)
        holidays.Add(GetNthWeekdayOfMonth(year, 9, DayOfWeek.Monday, 1));

        // Thanksgiving (4th Thursday in November)
        holidays.Add(GetNthWeekdayOfMonth(year, 11, DayOfWeek.Thursday, 4));

        // Christmas (December 25, or observed)
        var christmas = new DateTime(year, 12, 25);
        holidays.Add(AdjustForWeekend(christmas));

        return holidays;
    }

    private DateTime AdjustForWeekend(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday)
            return date.AddDays(-1); // Observe on Friday
        if (date.DayOfWeek == DayOfWeek.Sunday)
            return date.AddDays(1); // Observe on Monday
        return date;
    }

    private DateTime GetNthWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek, int n)
    {
        var firstDay = new DateTime(year, month, 1);
        var daysUntilTarget = ((int)dayOfWeek - (int)firstDay.DayOfWeek + 7) % 7;
        return firstDay.AddDays(daysUntilTarget + (n - 1) * 7);
    }

    private DateTime GetLastWeekdayOfMonth(int year, int month, DayOfWeek dayOfWeek)
    {
        var lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));
        var daysBack = ((int)lastDay.DayOfWeek - (int)dayOfWeek + 7) % 7;
        return lastDay.AddDays(-daysBack);
    }

    private DateTime GetGoodFriday(int year)
    {
        // Easter calculation using Meeus/Jones/Butcher algorithm
        var a = year % 19;
        var b = year / 100;
        var c = year % 100;
        var d = b / 4;
        var e = b % 4;
        var f = (b + 8) / 25;
        var g = (b - f + 1) / 3;
        var h = (19 * a + b - d - g + 15) % 30;
        var i = c / 4;
        var k = c % 4;
        var l = (32 + 2 * e + 2 * i - h - k) % 7;
        var m = (a + 11 * h + 22 * l) / 451;
        var month = (h + l - 7 * m + 114) / 31;
        var day = ((h + l - 7 * m + 114) % 31) + 1;

        var easter = new DateTime(year, month, day);
        return easter.AddDays(-2); // Good Friday is 2 days before Easter
    }
}

public class MarketStatusResponse
{
    [JsonPropertyName("market")]
    public string Market { get; set; } = string.Empty;

    [JsonPropertyName("serverTime")]
    public string ServerTime { get; set; } = string.Empty;

    [JsonPropertyName("nyseStatus")]
    public string NyseStatus { get; set; } = string.Empty;

    [JsonPropertyName("nasdaqStatus")]
    public string NasdaqStatus { get; set; } = string.Empty;

    [JsonPropertyName("isOpen")]
    public bool IsOpen { get; set; }

    [JsonPropertyName("source")]
    public string Source { get; set; } = string.Empty;
}

public class PolygonMarketStatusResponse
{
    [JsonPropertyName("market")]
    public string Market { get; set; } = string.Empty;

    [JsonPropertyName("serverTime")]
    public string ServerTime { get; set; } = string.Empty;

    [JsonPropertyName("exchanges")]
    public PolygonExchanges? Exchanges { get; set; }
}

public class PolygonExchanges
{
    [JsonPropertyName("nyse")]
    public string Nyse { get; set; } = string.Empty;

    [JsonPropertyName("nasdaq")]
    public string Nasdaq { get; set; } = string.Empty;

    [JsonPropertyName("otc")]
    public string? Otc { get; set; }
}
