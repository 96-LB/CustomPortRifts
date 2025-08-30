using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;

namespace CustomPortRifts.Patches;


public class BeatmapState : State<RRBeatmapPlayer, BeatmapState> {
    public AnimatorState? Counterpart { get; set; }
    public AnimatorState? Hero { get; set; }
}

[HarmonyPatch(typeof(RRBeatmapPlayer))]
public static class BeatmapPatch {
    [HarmonyPatch(nameof(RRBeatmapPlayer.ProcessBeatEvent))]
    [HarmonyPostfix]
    public static void ProcessBeatEvent(RRBeatmapPlayer __instance, BeatmapEvent beatEvent) {
        Plugin.Log.LogMessage($"ProcessBeatEvent: {beatEvent.type}");
        Plugin.Log.LogMessage(__instance._activeBeatmap.events.Count);
        var state = BeatmapState.Of(__instance);
        var beatmapEvent = beatEvent; // avoids false positive harmony warnings
        if(beatmapEvent.type == Constants.EVENT_SETPORTRAIT) {
            var name = beatmapEvent.GetFirstEventDataAsString(Constants.KEY_PORTRAITNAME);
            if(name == null) {
                return;
            }
            var isHero = beatmapEvent.GetFirstEventDataAsBool(Constants.KEY_ISHERO) ?? false;
            var animator = isHero ? state.Hero : state.Counterpart;
            animator?.SwitchPortrait(name);
        }
    }
}
