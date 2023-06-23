using Azure.Security.KeyVault.Secrets;

namespace Shared.Services.Azure;

public interface IKeyVaultService
{
    ValueTask<KeyVaultSecret> ReadSecret(string secretName);
}