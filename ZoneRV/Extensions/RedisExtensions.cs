using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using ZoneRV.Attributes;
using ZoneRV.Models;

namespace ZoneRV.Extensions;

public static class RedisExtensions
{
    private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings()
    {
        ContractResolver = new ZoneRVJsonResolver(JsonIgnoreType.Cache)
    };
    
    public static async Task SetRecordAsync<T>(this IDistributedCache cache, string recordId, T data,
        DistributedCacheEntryOptions entryOptions)
    {
        var json = JsonConvert.SerializeObject(data, _settings);
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