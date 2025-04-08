using CustomPortRifts.JsonConverters;
using Newtonsoft.Json;
using System;
using UnityEngine;

namespace CustomPortRifts;


[JsonObject(MemberSerialization.Fields)]
public readonly struct Settings {
    [JsonObject(MemberSerialization.Fields)]
    public readonly struct PortraitSettings {
        [JsonObject(MemberSerialization.Fields)]
        public readonly struct PoseSettings {
            [JsonObject(MemberSerialization.Fields)]
            public readonly struct SpriteSettings {
                public readonly float x;
                public readonly float y;
            }
            public readonly SpriteSettings[] sprites;
            public readonly float x;
            public readonly float y;

            public SpriteSettings GetSprite(int sprite) {
                return (sprites != null && 0 <= sprite && sprite < sprites.Length) ? sprites[sprite] : new();
            }
        }

        public readonly PoseSettings normal;
        public readonly PoseSettings doingPoorly;
        public readonly PoseSettings doingWell;
        public readonly PoseSettings vibePower;

        public readonly string character;
        public readonly float x;
        public readonly float y;

        public PoseSettings GetPose(PoseType pose) {
            return pose switch {
                PoseType.Normal => normal,
                PoseType.DoingPoorly => doingPoorly,
                PoseType.DoingWell => doingWell,
                PoseType.VibePower => vibePower,
                _ => throw new ArgumentException($"Invalid pose: {pose}")
            };
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    public readonly struct BackgroundSettings {
        [JsonObject(MemberSerialization.Fields)]
        public readonly struct ParticleSettings {
            public readonly string character;
            public readonly float? rotation;
            [JsonConverter(typeof(ColorConverter))] public readonly Color? color1;
            [JsonConverter(typeof(ColorConverter))] public readonly Color? color2;
            [JsonConverter(typeof(ColorConverter))] public readonly Color? colorOverTime;
        }

        public readonly ParticleSettings particles;

        public readonly string character;
        [JsonConverter(typeof(ColorConverter))] public readonly Color? bgColor1;
        [JsonConverter(typeof(ColorConverter))] public readonly Color? bgColor2;
        public readonly float? rotategradient;
        public readonly float? intensity;
        public readonly float? additiveintensity;





        [JsonConverter(typeof(ColorConverter))] public readonly Color? color1;
        [JsonConverter(typeof(ColorConverter))] public readonly Color? color2;
        [JsonConverter(typeof(ColorConverter))] public readonly Color? colorOverTime;
        [JsonConverter(typeof(ColorConverter))] public readonly Color? glow;
    }

    public readonly PortraitSettings hero;
    public readonly PortraitSettings counterpart;
    public readonly BackgroundSettings background;

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
