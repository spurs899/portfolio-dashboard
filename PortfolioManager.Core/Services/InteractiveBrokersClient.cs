using PortfolioManager.Contracts.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace PortfolioManager.Core.Services;

public interface IInteractiveBrokersClient
{
    Task<IBLoginResponse?> LoginAsync(string username, string password);
    Task<IBProfileResponse?> GetProfileAsync();
    Task<IBPortfolioResponse?> GetPortfolioAsync();
}

public class InteractiveBrokersClient : IInteractiveBrokersClient
{
    private readonly HttpClient _httpClient;
    private string? _authToken;

    public InteractiveBrokersClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IBLoginResponse?> LoginAsync(string username, string password)
    {
        // TODO: Implement IB authentication
        return null;
    }

    public async Task<IBProfileResponse?> GetProfileAsync()
    {
        // TODO: Implement IB profile retrieval
        return null;
    }

    public async Task<IBPortfolioResponse?> GetPortfolioAsync()
    {
        // TODO: Implement IB portfolio retrieval
        return null;
    }
}

//TODO:
// call https://www.interactivebrokers.com.au/sso/Authenticator to use QR code (re-direct/new tab)
// after qrcode provided https://www.interactivebrokers.com.au/portal.proxy/v1/portal/sso/validate
//Account info: https://www.interactivebrokers.com.au/portal.proxy/v1/portal/portfolio2/accounts
//Position info: https://www.interactivebrokers.com.au/portal.proxy/v1/portal/portfolio2/U5435267/positions
