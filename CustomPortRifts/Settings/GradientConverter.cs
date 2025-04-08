using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomPortRifts.Settings;


public class GradientConverter : JsonConverter<Gradient> {
    public override Gradient ReadJson(JsonReader reader, Type objectType, Gradient existingValue, bool hasExistingValue, JsonSerializer serializer) {
        var colors = new List<Color>();
        if(reader.TokenType == JsonToken.StartArray) {
            // treat as list of colors
            // TODO: add better support
            while(reader.Read()) {
                if(reader.TokenType == JsonToken.EndArray) {
                    break;
                }
                colors.Add(ColorConverter.Instance.ReadJson(reader, objectType, default, false, serializer));
            }
        } else if(reader.TokenType == JsonToken.String) {
            colors.Add(ColorConverter.Instance.ReadJson(reader, objectType, default, false, serializer));
        } else {
            Plugin.Log.LogError($"When parsing gradient at {reader.Path}, expected string or array but got {reader.TokenType} instead.");
            return existingValue;
        }

        var count = Mathf.Max(1f, colors.Count - 1); // max prevents divide by 0 error
        var colorKeys = colors.Select((x, i) => new GradientColorKey(x, i / count)).ToArray();
        var alphaKeys = colors.Select((x, i) => new GradientAlphaKey(x.a, i / count)).ToArray();
        var gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);
        Plugin.Log.LogMessage(reader.Path);
        DebugUtil.Dump(gradient);
        return gradient;
    }

    public override void WriteJson(JsonWriter writer, Gradient value, JsonSerializer serializer) {
        Plugin.Log.LogWarning($"[TODO: WRITE GRADIENT]"); // TODO: implement this
        writer.WriteNull();
        return;
    }
}
