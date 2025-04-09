using HarmonyLib;
using RhythmRift;

namespace CustomPortRifts.Patches;


using P = RRPortraitView;
using PortraitState = State<RRPortraitView, PortraitData>;
public class PortraitData {
    public Portrait Portrait { get; set; }

    public bool HasSprites => Portrait?.HasSprites ?? false;
}

[HarmonyPatch(typeof(P), nameof(P.PerformanceLevelChange))]
internal static class RRPortraitViewPatch {
    public static bool Prefix(
        P __instance
    ) {
        // suppress animator/voiceline changes if we're using custom sprites
        return !PortraitState.Of(__instance).HasSprites;
    }
}
