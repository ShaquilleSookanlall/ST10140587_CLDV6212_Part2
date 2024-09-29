using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

public static class SessionExtensions
{
    public static void SetObjectAsJson(this ISession session, string key, object value)
    {
        var serializedValue = JsonConvert.SerializeObject(value); // Use JsonConvert from Newtonsoft
        session.SetString(key, serializedValue);
    }

    public static T GetObjectFromJson<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value); // Use JsonConvert from Newtonsoft
    }
}
