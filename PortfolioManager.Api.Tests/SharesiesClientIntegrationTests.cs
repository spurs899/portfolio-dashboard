using Microsoft.Extensions.Configuration;
using PortfolioManager.Api.Services;
using FluentAssertions;
using System.Net;

namespace PortfolioManager.Api.Tests;

public class SharesiesClientIntegrationTests
{
    private readonly SharesiesClient _sut;
    private readonly string? _email;
    private readonly string? _password;

    public SharesiesClientIntegrationTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        _email = configuration["Sharesies:Email"] ?? Environment.GetEnvironmentVariable("SHARESIES_EMAIL");
        _password = configuration["Sharesies:Password"] ?? Environment.GetEnvironmentVariable("SHARESIES_PASSWORD");

        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer()
        };
        var httpClient = new HttpClient(handler);
        _sut = new SharesiesClient(httpClient);
    }

    [Fact(Skip = "Requires actual Sharesies credentials")]
    public async Task FullIntegrationFlow_ShouldSucceed()
    {
        if (string.IsNullOrEmpty(_email) || string.IsNullOrEmpty(_password))
        {
            return;
        }

        // 1. Login
        var loginResult = await _sut.LoginAsync(_email, _password);
        loginResult.Should().BeTrue("Login should succeed with valid credentials");

        // 2. Get Profile
        var profile = await _sut.GetProfileAsync();
        profile.Should().NotBeNull("Profile should be retrieved after login");
        profile!.User.Should().NotBeNull();
        profile.User!.Email.Should().Be(_email);

        // 3. Get Portfolio
        var portfolio = await _sut.GetPortfolioAsync();
        portfolio.Should().NotBeNull("Portfolio should be retrieved after login");
        portfolio!.Positions.Should().NotBeNull();
    }
}
