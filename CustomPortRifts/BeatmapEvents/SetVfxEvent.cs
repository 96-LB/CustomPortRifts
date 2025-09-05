namespace CustomPortRifts.BeatmapEvents;


public class SetVfxEvent : CustomEvent {
    public override string Type => "SetVFX";
    public string Name => GetString("Name");

    public override bool IsValid() {
        return base.IsValid() && !string.IsNullOrWhiteSpace(Name);
    }
}
