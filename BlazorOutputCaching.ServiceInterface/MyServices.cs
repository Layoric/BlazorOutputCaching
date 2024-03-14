using ServiceStack;
using BlazorOutputCaching.ServiceModel;
using Microsoft.AspNetCore.OutputCaching;

namespace BlazorOutputCaching.ServiceInterface;

[OutputCache(Duration = 60)]
public class MyServices : Service
{
    public object Any(Hello request)
    {
        return new HelloResponse { Result = $"Hello, {request.Name}!" };
    }
}