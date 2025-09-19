using CustomPortRifts.BeatmapEvents;
using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;

namespace CustomPortRifts.Patches;


public class BeatmapState : State<RRBeatmapPlayer, BeatmapState> {
    public PortraitViewState? Counterpart { get; set; }
    public PortraitViewState? Hero { get; set; }
    public StageState? Stage { get; set; }
}

[HarmonyPatch(typeof(RRBeatmapPlayer))]
public static class BeatmapPatch {
    [HarmonyPatch(nameof(RRBeatmapPlayer.ProcessBeatEvent))]
    [HarmonyPostfix]
    public static void ProcessBeatEvent(RRBeatmapPlayer __instance, BeatmapEvent beatEvent) {
        var state = BeatmapState.Of(__instance);
        var start = (float)beatEvent.endBeatNumber; // not a typo
        if(CustomEvent.TryParse(beatEvent, out SetPortraitEvent setPortraitEvent)) {
            var animator = setPortraitEvent.IsHero ? state.Hero : state.Counterpart;
            var duration = setPortraitEvent.FadeTime;
            animator?.SetPortrait(setPortraitEvent.Name, start, duration);
        } else if(CustomEvent.TryParse(beatEvent, out SetVfxEvent setVfxEvent)) {
            var duration = setVfxEvent.TransitionDuration;
            var particleFade = setVfxEvent.ParticleFadeTime;
            state.Stage?.SetVfxConfig(setVfxEvent.Name, start, duration, particleFade);
        }
    }
}
