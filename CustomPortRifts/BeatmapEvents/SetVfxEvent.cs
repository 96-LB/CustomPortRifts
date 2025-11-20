namespace CustomPortRifts.BeatmapEvents;

// TODO: these should not trigger in practice mode
public class SetVfxEvent : CustomEvent {
    public override string Type => "SetVFX";
    public string Name => GetString("VFXName");
    public float TransitionDuration => GetFloat("TransitionDuration") ?? 0;

    public override bool IsValid() => base.IsValid() && !string.IsNullOrWhiteSpace(Name);
}
