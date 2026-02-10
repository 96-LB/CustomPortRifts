using System.Threading.Tasks;
using RiftOfTheNecroManager.BeatmapEvents;
using RiftOfTheNecroManager.Patches;

namespace CustomPortRifts.BeatmapEvents;


[CustomEvent("SetPortrait", flags: CustomEventFlags.SkipBeat0)]
public class SetPortraitEvent : CustomEvent {
    public string Name => GetString("PortraitName");
    public bool IsHero => GetBool("IsHero") ?? false;
    public float TransitionDuration => GetFloat("TransitionDuration") ?? 0;
    
    public float PortraitChangeBeat => Beat + TransitionDuration / 2;

    public override bool IsValid() => base.IsValid() && !string.IsNullOrWhiteSpace(Name);
    
    public bool Preloaded { get; private set; } = false;
    
    public override bool ShouldPreload(StageState stage) {
        var state = Patches.StageState.Of(stage.Instance);
        return state.ShouldUseCustomGraphics && (base.ShouldPreload(stage) || state.ShouldPreload(this));
    }
    
    public override async Task Preload(StageState stage) {
        var state = Patches.StageState.Of(stage.Instance);
        var animator = IsHero ? state.Hero : state.Counterpart;
        await animator.PreloadPortrait(state.BasePortraitPath, Name);
        Preloaded = true;
   }
    
    public override void Process(StageState stage) {
        var state = Patches.StageState.Of(stage.Instance);
        var portrait = IsHero ? state.Hero : state.Counterpart;
        var duration = TransitionDuration;
        portrait?.SetPortrait(Name, Beat, duration);
    }
    
    public override void Skip(StageState stage) {
        if(!Preloaded) {
            return;
        }
        
        var state = Patches.StageState.Of(stage.Instance);
        Process(stage);
        state.Hero.UpdateTransitions(stage.StartBeat);
        state.Counterpart.UpdateTransitions(stage.StartBeat);
    }
}
