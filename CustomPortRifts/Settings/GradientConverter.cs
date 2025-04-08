using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace CustomPortRifts.Settings;


public class GradientConverter : JsonConverter<Gradient> {
    private static readonly ColorConverter colorConverter = new();

    public override Gradient ReadJson(JsonReader reader, Type objectType, Gradient existingValue, bool hasExistingValue, JsonSerializer serializer) {
        var colors = new List<Color>();
        if(reader.TokenType == JsonToken.StartArray) {
            // treat as list of colors
            // TODO: add better support
            while(reader.Read()) {
                if(reader.TokenType == JsonToken.EndArray) {
                    break;
                }
                colors.Add(colorConverter.ReadJson(reader, objectType, default, false, serializer));
            }
        } else if(reader.TokenType == JsonToken.String) {
            colors.Add(colorConverter.ReadJson(reader, objectType, default, false, serializer));
        } else {
            Plugin.Log.LogError($"When parsing gradient at {reader.Path}, expected string or array but got {reader.TokenType} instead.");
            return existingValue;
        }

        var count = Mathf.Max(2, colors.Count); // max prevents divide by 0 error
        var colorKeys = colors.Select((x, i) => new GradientColorKey(x, i / (float)count)).ToArray();
        var alphaKeys = colors.Select((x, i) => new GradientAlphaKey(x.a, i / (float)count)).ToArray();
        var gradient = new Gradient();
        gradient.SetKeys(colorKeys, alphaKeys);
        return gradient;
    }

    public override void WriteJson(JsonWriter writer, Gradient value, JsonSerializer serializer) {
        Plugin.Log.LogWarning($"[TODO: WRITE GRADIENT]"); // TODO: implement this
        writer.WriteNull();
        return;
    }
}
