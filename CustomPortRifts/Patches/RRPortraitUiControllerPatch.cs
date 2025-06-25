using HarmonyLib;
using RhythmRift;
using Shared.DLC;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections;
using System.IO;
using UnityEngine;

namespace CustomPortRifts.Patches;


[HarmonyPatch(typeof(RRPortraitUiController))]
internal static class RRPortraitUiControllerPatch {
    [HarmonyPatch(nameof(RRPortraitUiController.LoadCharacterPortrait))]
    [HarmonyPrefix]
    public static void LoadCharacterPortrait_Pre(
        RRPortraitUiController __instance,
        bool isHeroPortrait,
        ref string characterId,
        ref DataDrivenAnimator.Options portraitOptions,
        ref ITrackPortrait portraitMetadata,
        ref bool __state
     ) {
        Plugin.Log.LogMessage(characterId);
        if(Config.Cadence.Crypt.Value && isHeroPortrait && characterId == __instance.CadenceDefaultPortraitCharacterId || characterId == DlcController.Instance.GetSupporterRRCharacterName()) {
            characterId = "CadenceCrypt";
        }
        var path = Path.Combine(Path.GetDirectoryName(Application.dataPath), Plugin.NAME, characterId);
        Plugin.Log.LogMessage(path);
        if(FileUtils.IsDirectory(path)) {
            Plugin.Log.LogMessage("found!");
            var portrait = LocalTrackPortrait.TryLoadCustomPortrait(path, isHeroPortrait ? "CustomHero" : "CustomCounterpart");
            if(portrait != null) {
                Plugin.Log.LogMessage("wahoo!");
                characterId = portrait.PortraitId;
                portraitMetadata = portrait;
                portraitOptions = __instance.InitCustomPortrait(portrait);
                if(isHeroPortrait) {
                    __instance._heroPortraitId = characterId;
                } else {
                    __instance._counterpartPortraitId = characterId;
                }
                __state = true; // Indicate that a custom portrait was loaded
            }
        }
    }

    [HarmonyPatch(nameof(RRPortraitUiController.LoadCharacterPortrait))]
    [HarmonyPostfix]
    public static void LoadCharacterPortrait_Post(
        RRPortraitUiController __instance, 
        DataDrivenAnimator.Options portraitOptions, 
        bool __state,
        ref IEnumerator __result
    ) {
        // since the original function is a coroutine, we need to wrap the output to properly postfix
        var original = __result;
        __result = Wrapper();

        IEnumerator Wrapper() {
            if(__state) {
                Plugin.Log.LogMessage("super wahoo!");
                yield return __instance.PreloadCustomPortrait(portraitOptions);
            }
            yield return original;
        }
    }
}
