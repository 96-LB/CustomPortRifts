using Newtonsoft.Json;

namespace CustomPortRifts.Settings;


[JsonObject(MemberSerialization.Fields)]
public readonly struct PoseSettings {
    public readonly SpriteSettings[] sprites;
    public readonly float x;
    public readonly float y;

    public SpriteSettings GetSprite(int sprite) {
        return sprites != null && 0 <= sprite && sprite < sprites.Length ? sprites[sprite] : new();
    }
}
