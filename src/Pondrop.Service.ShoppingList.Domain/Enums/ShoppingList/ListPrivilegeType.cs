using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;

[JsonConverter(typeof(ListPrivilegeTypeEnumConverter))]
public enum ListPrivilegeType
{
    unknown,
    admin,
    editor,
    add,
    view
}

internal class ListPrivilegeTypeEnumConverter : StringEnumConverter
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            if (string.IsNullOrEmpty(reader?.Value?.ToString()))
                return ListPrivilegeType.unknown;

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch
        {
            return ListPrivilegeType.unknown;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        base.WriteJson(writer, value, serializer);
    }
}