using HarmonyLib;
using RhythmRift;

namespace CustomPortRifts.Patches;


using P = RRPortraitView;

[HarmonyPatch(typeof(P), nameof(P.PerformanceLevelChange))]
internal static class RRPortraitViewPatch {
    public static bool Prefix(
        P __instance
    ) {
        return !CustomPortraits.UsingCustomSprites || __instance != CustomPortraits.Portrait;
    }
}
