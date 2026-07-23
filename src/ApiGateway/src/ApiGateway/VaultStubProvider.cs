using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace ApiGateway
{
    // ============================================================================
    // EDU: SECRETS MANAGEMENT (Key Vault Provider Pattern)
    // ============================================================================
    // In an enterprise system, secrets (like JWT Keys, DB Passwords) are never
    // stored in source control or plain appsettings.json.
    // They are loaded at startup from a secure store like Azure Key Vault or HashiCorp Vault.
    // This custom ConfigurationProvider stubs that behavior, injecting a secret dynamically.
    // ============================================================================
    public class VaultStubConfigurationSource : IConfigurationSource
    {
        public IConfigurationProvider Build(IConfigurationBuilder builder) => new VaultStubConfigurationProvider();
    }

    public class VaultStubConfigurationProvider : ConfigurationProvider
    {
        public override void Load()
        {
            // Simulate fetching a secret from Azure Key Vault over the network
            Data = new Dictionary<string, string>
            {
                { "JwtSecretFromVault", "SuperSecretKeyThatNeedsToBeLongEnoughForHS256" }
            };
        }
    }

    public static class VaultStubExtensions
    {
        public static IConfigurationBuilder AddVaultStub(this IConfigurationBuilder builder)
        {
            return builder.Add(new VaultStubConfigurationSource());
        }
    }
}
