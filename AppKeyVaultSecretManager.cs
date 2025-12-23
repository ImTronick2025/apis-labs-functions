using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Security.KeyVault.Secrets;

namespace ApisLabs.Functions;

public sealed class AppKeyVaultSecretManager : KeyVaultSecretManager
{
    public override string GetKey(KeyVaultSecret secret)
    {
        var name = secret.Name;

        if (name.Equals("cosmosdb-connection-string", StringComparison.OrdinalIgnoreCase))
        {
            return "CosmosDbConnectionString";
        }

        if (name.Equals("cosmosdb-primary-key", StringComparison.OrdinalIgnoreCase))
        {
            return "CosmosDbPrimaryKey";
        }

        if (name.Equals("appinsights-connection-string", StringComparison.OrdinalIgnoreCase))
        {
            return "APPLICATIONINSIGHTS_CONNECTION_STRING";
        }

        if (name.Equals("functions-storage-connection-string", StringComparison.OrdinalIgnoreCase))
        {
            return "FunctionsStorageConnectionString";
        }

        return base.GetKey(secret);
    }
}
