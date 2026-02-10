using Godot;
using System.Text.Json;

namespace GodotMcp.EditorPlugin.Utils;

public static class VariantUtils
{
    public static Variant ConvertToVariant(JsonElement json)
    {
        switch (json.ValueKind)
        {
            case JsonValueKind.String:
                string s = json.GetString();
                if (s.StartsWith("res://"))
                {
                    var res = GD.Load(s);
                    if (res != null) return res;
                }
                return s;
            case JsonValueKind.Number:
                if (json.TryGetInt64(out long l)) return l;
                return json.GetDouble();
            case JsonValueKind.True: return true;
            case JsonValueKind.False: return false;
            case JsonValueKind.Null: return new Variant();
            case JsonValueKind.Array:
                 var arr = new Godot.Collections.Array();
                 foreach (var item in json.EnumerateArray()) arr.Add(ConvertToVariant(item));
                 return arr;
            case JsonValueKind.Object:
                 var dict = new Godot.Collections.Dictionary();
                 foreach (var prop in json.EnumerateObject())
                 {
                     dict[prop.Name] = ConvertToVariant(prop.Value);
                 }
                 return dict;
            default:
                return new Variant();
        }
    }
}
