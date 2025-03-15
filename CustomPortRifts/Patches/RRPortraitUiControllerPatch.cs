using System.Collections;
using HarmonyLib;
using RhythmRift;
using UnityEngine.UI;

namespace CustomPortRifts.Patches;


using P = RRPortraitUiController;

[HarmonyPatch(typeof(P))]
internal static class RRPortraitUiControllerPatch {
    [HarmonyPatch(nameof(P.UpdateDisplay))]
    [HarmonyPostfix]
    public static void UpdateDisplay(
        RRPerformanceLevel performanceLevel
    ) {
        BeatmapAnimatorControllerPatch.performanceLevel = performanceLevel;
    }

    [HarmonyPatch("LoadCharacterPortrait")]
    [HarmonyPostfix]
    public static void LoadCharacterPortrait(
        P __instance,
        bool isHeroPortrait,
        ref IEnumerator __result
    ) {
        if(!BeatmapAnimatorControllerPatch.UsingCustomSprites || isHeroPortrait) {
            return;
        }

        var original = __result;
        IEnumerator wrapper() {
            // TODO: error handling
            // TODO: add something for onconfigchange
            yield return original;
            var portrait = __instance.Field<RRPortraitView>("_counterpartPortraitViewInstance").Value;
            DebugUtil.PrintAllChildren(portrait, true, true);
            var image = portrait.transform.Find("MaskImage").Find("CharacterImage").GetComponent<Image>();
            image.sprite = BeatmapAnimatorControllerPatch.normalSprites[0];
        }

        __result = wrapper(); // since this is an iterator, we need to wrap it to properly postfix
    }
}
