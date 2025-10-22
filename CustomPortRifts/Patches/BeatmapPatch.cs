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
            var portrait = setPortraitEvent.IsHero ? state.Hero : state.Counterpart;
            var duration = setPortraitEvent.TransitionDuration;
            portrait?.SetPortrait(setPortraitEvent.Name, start, duration);
        } else if(CustomEvent.TryParse(beatEvent, out SetVfxEvent setVfxEvent)) {
            var duration = setVfxEvent.TransitionDuration;
            state.Stage?.SetVfxConfig(setVfxEvent.Name, start, duration);
        } else if(CustomEvent.TryParse(beatEvent, out SetPortraitColorEvent setPortraitColorEvent)) {
            var portrait = setPortraitColorEvent.IsHero ? state.Hero : state.Counterpart;
            var color = setPortraitColorEvent.Color ?? default;
            var duration = setPortraitColorEvent.TransitionDuration;
            portrait?.SetPortraitColor(color, start, duration);
        }
    }
}
