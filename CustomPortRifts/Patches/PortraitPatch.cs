using HarmonyLib;
using Shared.TrackData;
using System.Linq;

namespace CustomPortRifts.Patches;



[HarmonyPatch(typeof(LocalTrackPortrait))]
public static class PortraitPatch {
    [HarmonyPatch(nameof(LocalTrackPortrait.TryLoadCustomPortrait))]
    [HarmonyPostfix]
    public static void TryLoadCustomPortrait(ref LocalTrackPortrait __result) {
        if(__result == null) {
            return;
        }

        const string NORM = "PerformingNormal";
        const string WELL = "PerformingWell";
        const string POOR = "PerformingPoorly";
        const string VIBE = "VibePower";
        var poses = new[] { NORM, WELL, POOR, VIBE };
        
        var animations = __result.CustomAnimations?.Animations;
        if(animations == null || !poses.Any(x => animations.ContainsKey(x))) {
            return;
        }

        // fill in any missing poses with copies of existing poses
        var fallback = animations.TryGetValue(NORM, out var norm) ? norm : 
            animations.TryGetValue(WELL, out var well) ? well :
            animations.TryGetValue(POOR, out var poor) ? poor :
            animations[VIBE];

        animations.TryAdd(NORM, fallback);
        animations.TryAdd(WELL, animations[NORM]);
        animations.TryAdd(POOR, animations[NORM]);
        animations.TryAdd(VIBE, animations[WELL]);
    }
}