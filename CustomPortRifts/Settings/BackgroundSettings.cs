using Newtonsoft.Json;
using UnityEngine;

namespace CustomPortRifts.Settings;


[JsonObject(MemberSerialization.Fields)]
public readonly struct BackgroundSettings {
    public readonly string character;

    [JsonConverter(typeof(ColorConverter))]
    public readonly Color? color;

    [JsonConverter(typeof(ColorConverter))]
    public readonly Color? highlightColor;
    
    public readonly float? intensity;
    public readonly float? intensity2;

    public readonly float? rotation;
}
