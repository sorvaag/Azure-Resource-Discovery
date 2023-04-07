using Microsoft.Extensions.Configuration;
namespace AzDiscovery.Console
{
    internal class SecretManager
    {
        internal static T ReadSection<T>(string sectionName)
        {
            var builder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>()
            .AddEnvironmentVariables();
            
            var configurationRoot = builder.Build();

            return configurationRoot.GetSection(sectionName).Get<T>();
        }
    }
}