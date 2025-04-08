using System;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomPortRifts.Settings;


public class ColorConverter : JsonConverter<Color> {
    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer) {
        if(reader.TokenType != JsonToken.String) {
            Plugin.Log.LogError($"When parsing color at {reader.Path}, expected string but got {reader.TokenType} instead.");
            return existingValue;
        }

        if(!ColorUtility.TryParseHtmlString(reader.Value as string, out Color color)) {
            Plugin.Log.LogError($"Failed to parse color {reader.Value} at {reader.Path}.");
            return existingValue;
        }

        return color;
    }

    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer) {
        string val = ColorUtility.ToHtmlStringRGBA(value);
        writer.WriteValue(val);
    }
}
