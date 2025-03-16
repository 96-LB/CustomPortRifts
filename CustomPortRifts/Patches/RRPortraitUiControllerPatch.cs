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
        if(!CustomPortraits.UsingCustomSprites || isHeroPortrait) {
            return;
        }

        var original = __result;
        IEnumerator wrapper() {
            yield return original;
            var portrait = __instance.Field<RRPortraitView>("_counterpartPortraitViewInstance").Value;
            portrait.Field<Animator>("_portraitAnimator").Value.enabled = false;

            var image = portrait.transform.Find("MaskImage").Find("CharacterImage").GetComponent<Image>();
            image.sprite = CustomPortraits.NormalSprites[0];
            image.preserveAspect = true;

            CustomPortraits.Portrait = portrait;
        }

        __result = wrapper(); // since this is an iterator, we need to wrap it to properly postfix
    }
}
