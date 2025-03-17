using System.Collections;
using HarmonyLib;
using RhythmRift;
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
        CustomPortraits.PerformanceLevel = performanceLevel;
    }

    [HarmonyPatch("LoadCharacterPortrait")]
    [HarmonyPrefix]
    public static void LoadCharacterPortrait_Pre(
        bool isHeroPortrait,
        ref string characterId
    ) {
        if(!CustomPortraits.UsingCustomSprites || isHeroPortrait) {
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
            
            if(!CustomPortraits.UsingCustomSprites || isHeroPortrait) {
                yield break;
            }

            var portrait = __instance._counterpartPortraitViewInstance;
            portrait._portraitAnimator.enabled = false;

            var image = portrait.transform.Find("MaskImage").Find("CharacterImage").GetComponent<Image>();
            image.sprite = CustomPortraits.NormalSprites[0];
            image.preserveAspect = true;
            image.GetComponent<RectTransform>().anchoredPosition += 100 * Vector2.up;

            CustomPortraits.Portrait = portrait;
        }
    }
}
