using Azure.Security.KeyVault.Secrets;

namespace Shared.Services.Azure;

internal sealed class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient secretClient;

    public KeyVaultService(SecretClient secretClient) =>
        this.secretClient = secretClient;
    public async ValueTask<KeyVaultSecret> ReadSecret(string secretName)
    {
        var secret = await secretClient.GetSecretAsync(secretName);
        return secret.Value;
    }
}