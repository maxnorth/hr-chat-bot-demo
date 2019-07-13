using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AisBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureAppConfiguration((context, config) =>
                {                
                    var builtConfig = config.Build();

                    var keyVaultName = builtConfig["keyVaultName"];
                    config.AddAzureKeyVault($"https://{keyVaultName}.vault.azure.net/");
                });
        }
    }
}
