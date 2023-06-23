using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Shared.Configuration;
using Shared.Http;
using Shared.Services.Azure;

namespace Shared.IoC;

public static class ServiceCollectionExtensions
{
    public static void AddTelemetry(
        this IServiceCollection services,
        string serviceName)
    {
        services.AddOpenTelemetry()
            .WithTracing(tracerProviderBuilder =>
                tracerProviderBuilder
                    .AddSource(serviceName)
                    .ConfigureResource(resource => resource.AddService(serviceName))
                    .AddAspNetCoreInstrumentation()
                    .AddConsoleExporter());
    }

    public static void AddAzureKeyVault(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var options = new SecretClientOptions()
        {
            Retry = {
                Delay= TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
            }
        };

        var client = new SecretClient(
            new Uri(configuration["KeyVault"] ?? throw new ArgumentNullException("KeyVault")),
            new DefaultAzureCredential(),
            options);

        services.AddSingleton(client);
        services.AddScoped<IKeyVaultService, KeyVaultService>();
    }

    public static void AddB2CAuthenticationWithJwt(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var idpCfg = new IdPConfig();
        configuration.GetSection("IdP").Bind(idpCfg);

        services.AddAuthentication().AddJwtBearer(opt =>
        {
            var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(idpCfg.DiscoveryEndpoint, new OpenIdConnectConfigurationRetriever());

            var config = configManager.GetConfigurationAsync().GetAwaiter().GetResult();

            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = idpCfg.Issuer,
                ValidateAudience = false,
                IssuerSigningKeys = config.SigningKeys
            };
        });
    }

    public static void AddOAuthHttpClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var idpConfigSection = configuration.GetSection("IdP");
        services.Configure<IdPConfig>(idpConfigSection);

        var idpCfg = new IdPConfig();
        idpConfigSection.Bind(idpCfg);

        services.AddHttpClient("OAuth", httpClient =>
        {
            httpClient.BaseAddress = idpCfg?.Uri
                ?? throw new ArgumentNullException(nameof(idpCfg.Uri));
        });
    }

    public static void RegisterAllServiceClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<GetAccessTokenHttpHandler>();

        var srvsCfg = new List<ServiceConfig>();
        configuration.GetSection("Services").Bind(srvsCfg);

        srvsCfg.ForEach(services.RegisterServiceClient);
    }

    private static void RegisterServiceClient(
        this IServiceCollection services,
        ServiceConfig srvCfg)
    {
        if (string.IsNullOrWhiteSpace(srvCfg?.Name))
            throw new ArgumentNullException(nameof(srvCfg.Name));

        services
            .AddHttpClient(srvCfg.Name, httpClient =>
            {
                httpClient.BaseAddress = srvCfg?.Uri
                    ?? throw new ArgumentNullException(nameof(srvCfg.Uri));
            })
            .ConfigurePrimaryHttpMessageHandler<GetAccessTokenHttpHandler>();
    }
}