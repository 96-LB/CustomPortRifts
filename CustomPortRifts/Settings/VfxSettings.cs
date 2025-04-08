using Newtonsoft.Json;
using UnityEngine;

namespace CustomPortRifts.Settings;


[JsonObject(MemberSerialization.Fields)]
public readonly struct VfxSettings {
    public readonly string character;

    public readonly ParticleSettings particles;
    public readonly BackgroundSettings background;
    public readonly GradientSettings clouds;
    public readonly GradientSettings speedlines;

    [JsonConverter(typeof(ColorConverter))]
    public readonly Color? rift;
}
