using Newtonsoft.Json.Linq;

namespace ZoneRV.Serialization;

public class LocationInfoJsonConverter : JsonConverter<LocationInfo>
{
    public override void WriteJson(JsonWriter writer, LocationInfo? value, JsonSerializer serializer)
    {
        if(value is null || value.CurrentLocation is null)
            return;

        var jsonObject = new JObject();
        
        jsonObject.Add("currentLocation", JToken.FromObject(value.CurrentLocation, serializer));
        jsonObject.Add("locationHistory", JToken.FromObject(value.ToArray(), serializer));

        jsonObject.WriteTo(writer);
    }

    public override LocationInfo? ReadJson(JsonReader reader, Type objectType, LocationInfo? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}