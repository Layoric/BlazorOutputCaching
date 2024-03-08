using ServiceStack.Auth;
using BlazorOutputCaching.Data;

[assembly: HostingStartup(typeof(BlazorOutputCaching.ConfigureAuth))]

namespace BlazorOutputCaching;

public class ConfigureAuth : IHostingStartup
{
    public void Configure(IWebHostBuilder builder) => builder
        .ConfigureServices(services =>
        {
            services.AddPlugin(new AuthFeature(IdentityAuth.For<ApplicationUser>(options => {
                options.SessionFactory = () => new CustomUserSession();
                options.CredentialsAuth();
                options.AdminUsersFeature();
            })));
        });
}
