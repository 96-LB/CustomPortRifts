using HarmonyLib;
using RhythmRift;

namespace CustomPortRifts.Patches;


using P = RRPortraitView;
using PortraitState = State<RRPortraitView, PortraitData>;
public class PortraitData {
    public Portrait Portrait { get; set; }

    public bool UsingCustomSprites => Portrait?.UsingCustomSprites ?? false;
}

[HarmonyPatch(typeof(P), nameof(P.PerformanceLevelChange))]
internal static class RRPortraitViewPatch {
    public static bool Prefix(
        P __instance
    ) {
        // suppress animator/voiceline changes if we're using custom sprites
        return !PortraitState.Of(__instance).UsingCustomSprites;
    }
}
