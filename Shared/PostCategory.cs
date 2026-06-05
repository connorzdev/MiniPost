using System.Text.Json.Serialization;

namespace Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum PostCategory
{
    Music,
    Pet,
    Other,
}
