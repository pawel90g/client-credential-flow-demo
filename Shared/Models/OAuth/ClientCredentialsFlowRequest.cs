namespace Shared.Models.OAuth;

internal sealed class ClientCredentialsFlowRequest
{
    private readonly string grantType = "client_credentials";
    private readonly string clientId;
    private readonly string clientSecret;
    private readonly string scope;

    public ClientCredentialsFlowRequest(string clientId, string clientSecret, string scope)
    {
        this.clientId = clientId;
        this.clientSecret = clientSecret;
        this.scope = scope;
    }

    public FormUrlEncodedContent ToFormUrlEncodedContent() =>
        new(new Dictionary<string, string>
        {
            {"grant_type", grantType},
            {"client_id", clientId},
            {"client_secret", clientSecret},
            {"scope", scope},
        });
}