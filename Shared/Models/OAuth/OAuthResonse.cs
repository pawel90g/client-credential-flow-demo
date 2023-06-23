using System.Text.Json.Serialization;

namespace Shared.Models.OAuth;

internal sealed class OAuthResponse
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; set; }
    [JsonPropertyName("token_type")] public string? TokenType { get; set; }
    [JsonPropertyName("not_before")] public long NotBefore { get; set; }
    [JsonPropertyName("expires_in")] public int ExpiresIn { get; set; }
    [JsonPropertyName("expires_on")] public long ExpiresOn { get; set; }
    [JsonPropertyName("resource")] public Guid Resource { get; set; }
}