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

public class RedisOutputCacheStore : IOutputCacheStore
{
    private readonly IRedisClientsManager _redisManager;

    public RedisOutputCacheStore(IRedisClientsManager redisManager)
    {
        _redisManager = redisManager;
    }

    public async ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken)
    {
        await using var redis =  await _redisManager.GetClientAsync(token: cancellationToken);
        var value = await redis.GetAsync<byte[]>(key, cancellationToken);
        return value;
    }

    public async ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken)
    {
        await using var redis = await _redisManager.GetClientAsync(token: cancellationToken);
        
        // First persist in normal cache hashset
        await redis.SetAsync(key, value, validFor, cancellationToken);

        if (tags == null)
            return;
        foreach (var tag in tags)
        {
            await redis.AddItemToSetAsync($"tag:{tag}", key, cancellationToken);
        }
    }

    public async ValueTask EvictByTagAsync(string tag, CancellationToken cancellationToken)
    {
        await using var redis = await _redisManager.GetClientAsync(token: cancellationToken);
        
        var keys = await redis.GetAllItemsFromListAsync($"tag:{tag}", cancellationToken);

        var strList = keys.Select(x => x.ConvertTo<string>());

        var pipelineAsync = redis.CreatePipeline();
        foreach (var key in strList)
        {
            pipelineAsync.QueueCommand(async => async.RemoveEntryAsync(key));
            pipelineAsync.QueueCommand(async => async.RemoveItemFromSetAsync($"tag:{tag}", key, cancellationToken));
        }

        await pipelineAsync.FlushAsync(cancellationToken);
    }
}
