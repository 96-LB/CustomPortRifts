using System.Threading.Tasks;
using RiftOfTheNecroManager.BeatmapEvents;
using RiftOfTheNecroManager.Patches;

namespace CustomPortRifts.BeatmapEvents;


// TODO: these should not trigger in practice mode
[CustomEvent("SetVFX", flags: CustomEventFlags.SkipBeat0)]
public class SetVfxEvent : CustomEvent {
    public string Name => GetString("VFXName");
    public float TransitionDuration => GetFloat("TransitionDuration") ?? 0;
    
    public override bool IsValid() => base.IsValid() && !string.IsNullOrWhiteSpace(Name);
    
    public override async Task Preload(StageState stage) { }
    
    public override void Process(StageState stage) {
        var state = Patches.StageState.Of(stage.Instance);
        var duration = TransitionDuration;
        state?.SetVfxConfig(Name, Beat, duration);
    }
    
    public override void Skip(StageState stage) {
        var state = Patches.StageState.Of(stage.Instance);
        Process(stage);
        state.Transition.Update(stage.StartBeat);
    }
}
