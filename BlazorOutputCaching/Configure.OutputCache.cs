using Microsoft.AspNetCore.OutputCaching;
using ServiceStack.Redis;

[assembly: HostingStartup(typeof(BlazorOutputCaching.ConfigureOutputCache))]

namespace BlazorOutputCaching;

public class ConfigureOutputCache : IHostingStartup
{
    public void Configure(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<IRedisClientsManager>(c =>
                new BasicRedisClientManager("localhost:6379"));
            services.AddSingleton<IOutputCacheStore, RedisOutputCacheStore>();
        });
    }
}

public class RedisOutputCacheStore(IRedisClientsManager redisManager) : IOutputCacheStore
{
    public async ValueTask<byte[]?> GetAsync(string key, CancellationToken cancellationToken)
    {
        await using var redis =  await redisManager.GetClientAsync(token: cancellationToken);
        var value = await redis.GetAsync<byte[]>(key, cancellationToken);
        return value;
    }

    public async ValueTask SetAsync(string key, byte[] value, string[]? tags, TimeSpan validFor, CancellationToken cancellationToken)
    {
        await using var redis = await redisManager.GetClientAsync(token: cancellationToken);
        
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
        await using var redis = await redisManager.GetClientAsync(token: cancellationToken);
        
        var keys = await redis.GetAllItemsFromListAsync($"tag:{tag}", cancellationToken);
        
        foreach (var key in keys)
        {
            await redis.RemoveEntryAsync(key);
            await redis.RemoveItemFromSetAsync($"tag:{tag}", key, cancellationToken);
        }
    }
}