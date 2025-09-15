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
        if(CustomEvent.TryParse(beatEvent, out SetPortraitEvent setPortraitEvent)) {
            var animator = setPortraitEvent.IsHero ? state.Hero : state.Counterpart;
            animator?.SetPortrait(setPortraitEvent.Name);
        } else if(CustomEvent.TryParse(beatEvent, out SetVfxEvent setVfxEvent)) {
            var start = (float)beatEvent.endBeatNumber;
            var duration = setVfxEvent.TransitionDuration;
            state.Stage?.SetVfxConfig(setVfxEvent.Name, start, duration);
        }
    }
}
