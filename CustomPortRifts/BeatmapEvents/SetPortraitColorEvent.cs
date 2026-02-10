using System.Threading.Tasks;
using RiftOfTheNecroManager.BeatmapEvents;
using RiftOfTheNecroManager.Patches;
using UnityEngine;

namespace CustomPortRifts.BeatmapEvents;


[CustomEvent("SetPortraitColor", flags: CustomEventFlags.SkipBeat0)]
public class SetPortraitColorEvent : CustomEvent {
    public Color? Color => GetColor("Color");
    public bool IsHero => GetBool("IsHero") ?? false;
    public float TransitionDuration => GetFloat("TransitionDuration") ?? 0;
    
    public override bool IsValid() => base.IsValid() && Color != null;
    
    public override async Task Preload(StageState stage) { }
    
    public override void Process(StageState stage) {
        var state = Patches.StageState.Of(stage.Instance);
        var portrait = IsHero ? state.Hero : state.Counterpart;
        var color = Color ?? default;
        portrait?.SetPortraitColor(color, Beat, TransitionDuration);
    }
    
    public override void Skip(StageState stage) {
        var state = Patches.StageState.Of(stage.Instance);
        Process(stage);
        state.Hero.UpdateTransitions(stage.StartBeat);
        state.Counterpart.UpdateTransitions(stage.StartBeat);
    }
}
