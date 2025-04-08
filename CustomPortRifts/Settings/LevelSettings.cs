using Newtonsoft.Json;
using UnityEngine;

namespace CustomPortRifts.Settings;


[JsonObject(MemberSerialization.Fields)]
public readonly struct LevelSettings {
    public readonly PortraitSettings hero;
    public readonly PortraitSettings counterpart;
    public readonly VfxSettings vfx;

    public readonly PortraitSettings GetPortraitSettings(bool isHero) {
        return isHero ? hero : counterpart;
    }

    public readonly Vector2 GetOffset(bool isHero, PoseType pose, int sprite) {
        var portraitSettings = GetPortraitSettings(isHero);
        var poseSettings = portraitSettings.GetPose(pose);
        var spriteSettings = poseSettings.GetSprite(sprite);
        return new(
            portraitSettings.x + poseSettings.x + spriteSettings.x,
            portraitSettings.y + poseSettings.y + spriteSettings.y
        );
    }
}
