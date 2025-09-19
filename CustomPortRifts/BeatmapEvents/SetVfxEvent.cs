namespace CustomPortRifts.BeatmapEvents;


public class SetVfxEvent : CustomEvent {
    public override string Type => "SetVFX";
    public string Name => GetString("Name");
    public float TransitionDuration => GetFloat("TransitionDuration") ?? 0;
    public float ParticleFade => GetFloat("ParticleFade") ?? 1;

    public override bool IsValid() {
        return base.IsValid() && !string.IsNullOrWhiteSpace(Name);
    }
}
