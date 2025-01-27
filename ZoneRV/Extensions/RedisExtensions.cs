using Microsoft.Extensions.Caching.Distributed;

namespace ZoneRV.Extensions;

public static class RedisExtensions
{
    public static async Task SetRecordAsync<T>(this IDistributedCache cache, string recordId, T data,
        DistributedCacheEntryOptions entryOptions)
    {
        var json = JsonConvert.SerializeObject(data);
        await cache.SetStringAsync(recordId, json, entryOptions);
    }

    public static async Task<T?> GetRecordAsync<T>(this IDistributedCache cache, string recordId)
    {
        var json = await cache.GetStringAsync(recordId);

        if (json is null)
        {
            return default(T);
        }

        return JsonConvert.DeserializeObject<T>(json);
    }
}