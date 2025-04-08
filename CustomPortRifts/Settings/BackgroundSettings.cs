using Newtonsoft.Json;
using System;
using System.Data;
using UnityEngine;

namespace CustomPortRifts.Settings;


[JsonObject(MemberSerialization.Fields)]
[JsonConverter(typeof(BackgroundConverter))]
public readonly struct BackgroundSettings {
    public readonly string character;

    [JsonConverter(typeof(ColorConverter))]
    public readonly Color? color;

    [JsonConverter(typeof(ColorConverter))]
    public readonly Color? highlightColor;
    
    public readonly float? intensity;
    public readonly float? intensity2;

    public readonly float? rotation;


    public BackgroundSettings() {}

    public BackgroundSettings(Color color) {
        // solid color background
        character = null;
        this.color = color;
        highlightColor = Color.clear;
        intensity = 0;
        intensity2 = 0;
        rotation = 0;
    }

    public class BackgroundConverter : JsonConverter<BackgroundSettings> {
        [ThreadStatic]
        private static bool disabled;

        public override bool CanRead => !disabled;
        public override bool CanWrite => false;
        

        public override BackgroundSettings ReadJson(JsonReader reader, Type objectType, BackgroundSettings existingValue, bool hasExistingValue, JsonSerializer serializer) {
            if(reader.TokenType != JsonToken.String) {
                // fall back to default
                // prevent stackoverflow using stackoverflow
                // https://stackoverflow.com/a/76705937/13231076
                try {
                    disabled = true;
                    var x = serializer.Deserialize<BackgroundSettings>(reader);
                    return x;
                } finally {
                    disabled = false;
                }
            }

            // this allows us to simply specify a solid color instead of a full object
            var color = ColorConverter.Instance.ReadJson(reader, objectType, default, false, serializer);
            return new(color);
        }

        public override void WriteJson(JsonWriter writer, BackgroundSettings value, JsonSerializer serializer) {
            throw new NotImplementedException();
        }
    }
}
