using UnityEngine;

namespace CustomPortRifts.BeatmapEvents;


public class SetPortraitColorEvent : CustomEvent {
    public override string Type => "SetPortraitColor";
    public Color? Color => GetColor("Color");
    public bool IsHero => GetBool("IsHero") ?? false;
    public float TransitionDuration => GetFloat("TransitionDuration") ?? 0;

    public override bool IsValid() => base.IsValid() && Color != null;
}
