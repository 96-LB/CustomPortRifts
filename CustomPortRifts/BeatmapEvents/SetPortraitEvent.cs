namespace CustomPortRifts.BeatmapEvents;


public class SetPortraitEvent : CustomEvent {
    public override string Type => "SetPortrait";
    public string Name => GetString("PortraitName");
    public bool IsHero => GetBool("IsHero") ?? false;


    public override bool IsValid() {
        return base.IsValid() && !string.IsNullOrWhiteSpace(Name);
    }
}
