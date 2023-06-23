using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Shared.Configuration;
using Shared.Models.OAuth;
using Shared.Services.Azure;
using Shared.Tools;

namespace Shared.Http;

internal sealed class GetAccessTokenHttpHandler : HttpClientHandler
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IKeyVaultService keyVaultService;
    private OAuthResponse oAuthResponse;
    private readonly IdPConfig idPConfig;

    public GetAccessTokenHttpHandler(
        IHttpClientFactory httpClientFactory,
        IOptions<IdPConfig> idpOptions,
        IKeyVaultService keyVaultService
        ) : base()
    {
        this.httpClientFactory = httpClientFactory;
        this.keyVaultService = keyVaultService;
        idPConfig = idpOptions.Value;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await SetBearerToken(request);
        return await base.SendAsync(request, cancellationToken);
    }

    protected override HttpResponseMessage Send(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        SetBearerToken(request).GetAwaiter().GetResult();
        return base.Send(request, cancellationToken);
    }

    private async Task<OAuthResponse> GetOAuthResponse()
    {
        var expiresOn = DateTimeTools.UnixTimeStampToUtcDateTime(oAuthResponse?.ExpiresOn);

        if (string.IsNullOrEmpty(oAuthResponse?.AccessToken)
            || !expiresOn.HasValue
            || expiresOn <= DateTime.UtcNow)
        {
            var oAuthClient = httpClientFactory.CreateClient("OAuth");

            var clientSecret = await keyVaultService.ReadSecret($"{idPConfig.ServiceName}-ClientSecret");

            if (clientSecret?.Value is null)
                throw new Exception("ClientSecret is not configured");

            var oAuthResponseMessage = await oAuthClient.PostAsync(
                "oauth2/v2.0/token",
                new ClientCredentialsFlowRequest(
                    idPConfig.ClientId,
                    clientSecret.Value,
                    idPConfig.Scope
                )
                .ToFormUrlEncodedContent());

            if (!oAuthResponseMessage.IsSuccessStatusCode)
                throw new Exception("Unable to retrieve access token");

            oAuthResponse = JsonSerializer.Deserialize<OAuthResponse>(
                await oAuthResponseMessage.Content.ReadAsStringAsync()
            );
        }

        return oAuthResponse;
    }

    private async Task SetBearerToken(HttpRequestMessage request)
    {
        var oAuthResponse = await GetOAuthResponse();

        request.Headers.Authorization = new AuthenticationHeaderValue(oAuthResponse.TokenType, oAuthResponse.AccessToken);
    }
}