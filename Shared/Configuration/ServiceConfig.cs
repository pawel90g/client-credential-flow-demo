namespace Shared.Configuration;

public sealed class ServicesConfig
{
    public List<ServiceConfig> Services { get; set; } = new List<ServiceConfig>();
}

public sealed class ServiceConfig
{
    public string? Name { get; set; }
    public string? Url { get; set; }
    public Uri? Uri => Url is null ? null : new(Url);
}