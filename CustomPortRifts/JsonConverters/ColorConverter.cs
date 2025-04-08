using System;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomPortRifts.JsonConverters;


public class ColorConverter : JsonConverter {
    public override bool CanConvert(Type objectType) {
        return typeof(Color).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
        if(reader.TokenType != JsonToken.String) {
            Plugin.Log.LogError($"When parsing color at {reader.Path}, expected string but got {reader.TokenType} instead.");
            return null;
        }

        if(!ColorUtility.TryParseHtmlString(reader.Value as string, out Color color)) {
            Plugin.Log.LogError($"Failed to parse color {reader.Value} at {reader.Path}.");
            return color;
        }
        
        return color;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
        string val = ColorUtility.ToHtmlStringRGBA((Color)value);
        writer.WriteValue(val);
    }
}
