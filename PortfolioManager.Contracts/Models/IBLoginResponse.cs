using System.Text.Json.Serialization;

namespace PortfolioManager.Contracts.Models;

public class IBLoginResponse
{
    [JsonPropertyName("authenticated")]
    public bool Authenticated { get; set; }

    [JsonPropertyName("token")]
    public string? Token { get; set; }
}
