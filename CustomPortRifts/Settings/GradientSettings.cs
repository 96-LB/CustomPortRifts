using Newtonsoft.Json;
using UnityEngine;

namespace CustomPortRifts.Settings;


[JsonObject(MemberSerialization.Fields)]
public readonly struct GradientSettings {
    [JsonConverter(typeof(GradientConverter))]
    public readonly Gradient color1;

    [JsonConverter(typeof(GradientConverter))]
    public readonly Gradient color2;

    [JsonConverter(typeof(GradientConverter))]
    public readonly Gradient colorOverTime;
}
