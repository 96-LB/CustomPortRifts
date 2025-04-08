using Newtonsoft.Json;

namespace CustomPortRifts.Settings;


[JsonObject(MemberSerialization.Fields)]
public readonly struct SpriteSettings {
    public readonly float x;
    public readonly float y;
}
