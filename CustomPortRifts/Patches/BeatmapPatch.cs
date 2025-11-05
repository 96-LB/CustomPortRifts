using CustomPortRifts.BeatmapEvents;
using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;

namespace CustomPortRifts.Patches;


public class BeatmapState : State<RRBeatmapPlayer, BeatmapState> {
    public StageState? Stage { get; set; }
    public PortraitViewState? Counterpart => Stage?.Counterpart;
    public PortraitViewState? Hero => Stage?.Hero;


    public void ProcessBeatEvent(BeatmapEvent beatEvent) {
        var start = (float)beatEvent.startBeatNumber;
        
        if(CustomEvent.TryParse(beatEvent, out SetPortraitEvent setPortraitEvent)) {
            if(setPortraitEvent.HasBeenProcessed()) {
                return;
            }
            setPortraitEvent.FlagAsProcessed();

            var portrait = setPortraitEvent.IsHero ? Hero : Counterpart;
            var duration = setPortraitEvent.TransitionDuration;
            portrait?.SetPortrait(setPortraitEvent.Name, start, duration);
        } else if(CustomEvent.TryParse(beatEvent, out SetVfxEvent setVfxEvent)) {
            // TODO: this can be handled way more DRY with a better TryParse and a switch
            if(setVfxEvent.HasBeenProcessed()) {
                return;
            }
            setVfxEvent.FlagAsProcessed();

            var duration = setVfxEvent.TransitionDuration;
            Stage?.SetVfxConfig(setVfxEvent.Name, start, duration);
        } else if(CustomEvent.TryParse(beatEvent, out SetPortraitColorEvent setPortraitColorEvent)) {
            if(setPortraitColorEvent.HasBeenProcessed()) {
                return;
            }
            setPortraitColorEvent.FlagAsProcessed();

            var portrait = setPortraitColorEvent.IsHero ? Hero : Counterpart;
            var color = setPortraitColorEvent.Color ?? default;
            var duration = setPortraitColorEvent.TransitionDuration;
            portrait?.SetPortraitColor(color, start, duration);
        }
    }

    public void ProcessBeatEvent(CustomEvent customEvent) {
        ProcessBeatEvent(customEvent.BeatmapEvent);
    }
}

[HarmonyPatch(typeof(RRBeatmapPlayer))]
public static class BeatmapPatch {
    [HarmonyPatch(nameof(RRBeatmapPlayer.ProcessBeatEvent))]
    [HarmonyPostfix]
    public static void ProcessBeatEvent(RRBeatmapPlayer __instance, BeatmapEvent beatEvent) {
        var state = BeatmapState.Of(__instance);
        state.ProcessBeatEvent(beatEvent);
    }
}
