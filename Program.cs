using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApisLabs.Functions;

public class Program
{
    public static void Main()
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                var builtConfig = config.Build();
                var keyVaultUri = builtConfig["KeyVaultUri"];

                if (!string.IsNullOrWhiteSpace(keyVaultUri))
                {
                    var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
                    config.AddAzureKeyVault(secretClient, new AppKeyVaultSecretManager());
                }
            })
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(services =>
            {
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
                services.AddSingleton<CosmosDbService>();
            })
            .Build();

        host.Run();
    }
}
