namespace CustomPortRifts.BeatmapEvents;


public class SetPortraitEvent : CustomEvent {
    public override string Type => "SetPortrait";
    public string Name => GetString("PortraitName");
    public bool IsHero => GetBool("IsHero") ?? false;
    public float TransitionDuration => GetFloat("TransitionDuration") ?? 0;


    public override bool IsValid() => base.IsValid() && !string.IsNullOrWhiteSpace(Name);
}
