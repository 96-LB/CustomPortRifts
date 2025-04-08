using Newtonsoft.Json;
using UnityEngine;

namespace CustomPortRifts.Settings;


[JsonObject(MemberSerialization.Fields)]
public readonly struct ParticleSettings {
    public readonly string character;
    public readonly float? rotation;
    public readonly GradientSettings color;
}
