using Microsoft.AspNetCore.OutputCaching;
using ServiceStack.Redis;

[assembly: HostingStartup(typeof(BlazorOutputCaching.AppHost))]

namespace BlazorOutputCaching;

public class AppHost() : AppHostBase("BlazorOutputCaching"), IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices((context, services) =>
        {
            // Configure ASP.NET Core IOC Dependencies
            services.AddPlugin(new AdminRedisFeature());
        });

    // Configure your AppHost with the necessary configuration and dependencies your App needs
    public override void Configure()
    {
        SetConfig(new HostConfig {
        });
    }
}


