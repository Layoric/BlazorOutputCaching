using ServiceStack;
using BlazorOutputCaching.ServiceModel;

namespace BlazorOutputCaching.ServiceInterface;

public class MyServices : Service
{
    public object Any(Hello request)
    {
        return new HelloResponse { Result = $"Hello, {request.Name}!" };
    }
}