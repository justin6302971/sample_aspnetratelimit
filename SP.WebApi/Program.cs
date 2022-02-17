using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SP.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var webHost=CreateHostBuilder(args).Build();

            using (var scope = webHost.Services.CreateScope())
            {
                // get the ClientPolicyStore instance
                var clientPolicyStore = scope.ServiceProvider.GetRequiredService<IClientPolicyStore>();

                // seed client data from appsettings
                clientPolicyStore.SeedAsync().GetAwaiter().GetResult();
            }
            
            webHost.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    
                });
    }
}
