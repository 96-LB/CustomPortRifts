using System.Collections;
using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;
using UnityEngine;
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
        Portrait.PerformanceLevel = performanceLevel;
    }

    [HarmonyPatch("LoadCharacterPortrait")]
    [HarmonyPrefix]
    public static void LoadCharacterPortrait_Pre(
        bool isHeroPortrait,
        ref string characterId
    ) {
        var portrait = isHeroPortrait ? Portrait.Hero : Portrait.Counterpart;
        if(!portrait.UsingCustomSprites) {
            return;
        }
        characterId = "Dove"; // every character has different portraits and animations. dove's is probably the nicest to work with
    }

    [HarmonyPatch("LoadCharacterPortrait")]
    [HarmonyPostfix]
    public static void LoadCharacterPortrait_Post(
        P __instance,
        bool isHeroPortrait,
        ref IEnumerator __result
    ) {
        // since the original function is a coroutine, we need to wrap the output to properly postfix
        var original = __result;
        __result = Wrapper();

        IEnumerator Wrapper() {
            yield return original;
            
            // TODO: we're repeating this
            var portrait = isHeroPortrait ? Portrait.Hero : Portrait.Counterpart;
            if(!portrait.UsingCustomSprites) {
                yield break;
            }

            var portraitView = isHeroPortrait ? __instance._heroPortraitViewInstance : __instance._counterpartPortraitViewInstance;
            portraitView._portraitAnimator.enabled = false;

            var image = portraitView.transform.Find("MaskImage").Find("CharacterImage").GetComponent<Image>();
            image.sprite = portrait.NormalSprites[0];
            image.preserveAspect = true;
            image.GetComponent<RectTransform>().anchoredPosition += 100 * Vector2.up;

            State<RRPortraitView, PortraitData>.Of(portraitView).Portrait = portrait;
            State<BeatmapAnimatorController, BeatmapData>.Of(portraitView.BeatmapAnimatorController).Portrait = portrait;
        }
    }
}
