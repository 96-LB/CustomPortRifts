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
    [HarmonyPatch(nameof(P.Initialize))]
    [HarmonyPostfix]
    public static void Initialize(
        ref IEnumerator __result
    ) {
        // since the original function is a coroutine, we need to wrap the output to properly postfix
        var original = __result;
        __result = Wrapper();

        IEnumerator Wrapper() {
            // prevents the portrait gameobjects from being loaded until we know whether we're using custom sprites
            yield return new WaitUntil(() => !Portrait.Loading);
            yield return original;
        }
    }


    [HarmonyPatch(nameof(P.UpdateDisplay))]
    [HarmonyPostfix]
    public static void UpdateDisplay(
        RRPerformanceLevel performanceLevel
    ) {
        Portrait.SetPerformanceLevel(performanceLevel);
    }

    //vanilla miss behaivour
    [HarmonyPatch("MissRecorded")]
    [HarmonyPrefix]
    public static void MissRecorded(){
        RRStageControllerPatch.lastVanillaMiss = RRStageControllerPatch.lastMiss;        
    }

    [HarmonyPatch("LoadCharacterPortrait")]
    [HarmonyPrefix]
    public static void LoadCharacterPortrait_Pre(
        bool isHeroPortrait,
        ref string characterId
    ) {
        if(!Portrait.Counterpart.HasSprites || isHeroPortrait) {
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
            
            var portrait = isHeroPortrait ? Portrait.Hero : Portrait.Counterpart;
            if(!portrait.HasSprites) {
                yield break;
            }

            var portraitView = isHeroPortrait ? __instance._heroPortraitViewInstance : __instance._counterpartPortraitViewInstance;
            Object.DestroyImmediate(portraitView._portraitAnimator);
            portraitView._portraitAnimator = portraitView.gameObject.AddComponent<Animator>(); // dummy to prevent null reference exceptions

            var image = portraitView.transform.Find("MaskImage").Find("CharacterImage").GetComponent<Image>();
            image.sprite = portrait.NormalSprites[0];
            image.preserveAspect = true;

            var portraitState = State<RRPortraitView, PortraitData>.Of(portraitView);
            portraitState.Portrait = portrait;

            var beatmapState = State<BeatmapAnimatorController, BeatmapData>.Of(portraitView.BeatmapAnimatorController);
            beatmapState.Portrait = portrait;
            beatmapState.Image = image;
        }
    }
}
