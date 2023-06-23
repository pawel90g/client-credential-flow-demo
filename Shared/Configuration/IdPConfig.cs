namespace Shared.Configuration;

public sealed class IdPConfig
{
    public string? ClientId { get; set; }
    public string? Url { get; set; }
    public Uri? Uri => Url is null ? null : new(Url);
    public string? Issuer { get; set; }
    public string? DiscoveryEndpoint { get; set; }
    public string? Scope { get; set; }
    public string? ServiceName { get; set; }
}