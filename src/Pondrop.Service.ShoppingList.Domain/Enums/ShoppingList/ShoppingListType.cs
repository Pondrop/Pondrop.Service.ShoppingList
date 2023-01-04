using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Pondrop.Service.ShoppingList.Domain.Enums.ShoppingList;

[JsonConverter(typeof(ShoppingListTypeEnumConverter))]
public enum ShoppingListType
{
    unknown,
    grocery
}

internal class ShoppingListTypeEnumConverter : StringEnumConverter
{
    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        try
        {
            if (string.IsNullOrEmpty(reader?.Value?.ToString()))
                return ShoppingListType.unknown;

            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch
        {
            return ShoppingListType.unknown;
        }
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        base.WriteJson(writer, value, serializer);
    }
}