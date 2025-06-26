using HarmonyLib;
using RhythmRift;
using Shared.DLC;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections;
using System.IO;
using UnityEngine;

namespace CustomPortRifts.Patches;


public class PortraitState : State<RRPortraitUiController, PortraitState> {
    public string LevelId { get; set; } = "";
    public string TrackName { get; set; } = "";
}


[HarmonyPatch(typeof(RRPortraitUiController))]
public static class RRPortraitUiControllerPatch {
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
        if(
            Config.Cadence.Crypt
            && isHeroPortrait
            && (characterId == __instance.CadenceDefaultPortraitCharacterId || characterId == DlcController.Instance.GetSupporterRRCharacterName())
        ) {
            characterId = "CadenceCrypt";
        }

        var state = PortraitState.Of(__instance);
        var basePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), Plugin.NAME);
        var portraitType = isHeroPortrait ? "Hero" : "Counterpart";
        var directory = "Custom" + portraitType;

        // we can load track by either id or track name
        var path = Path.Combine(basePath, "Tracks", state.LevelId, portraitType);
        if(!FileUtils.IsDirectory(path)) {
            path = Path.Combine(basePath, "Tracks", state.TrackName, portraitType);
        }

        // load the portrait from the track override
        LocalTrackPortrait? portrait = null;
        if(FileUtils.IsDirectory(path)) {
            portrait = LocalTrackPortrait.TryLoadCustomPortrait(path, directory);
        }

        // load the portrait from the character override
        path = Path.Combine(basePath, "Characters", portrait?.PortraitId ?? characterId);
        if(FileUtils.IsDirectory(path)) {
            portrait = LocalTrackPortrait.TryLoadCustomPortrait(path, directory);
        }

        // update metadata
        if(portrait != null) {
            characterId = portrait.PortraitId ?? characterId;
            portraitMetadata = portrait;
            portraitOptions = __instance.InitCustomPortrait(portrait);
            if(isHeroPortrait) {
                __instance._heroPortraitId = characterId;
            } else {
                __instance._counterpartPortraitId = characterId;
            }
            __state = true; // indicate that a custom portrait was loaded
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
                yield return __instance.PreloadCustomPortrait(portraitOptions);
            }
            yield return original;
        }
    }
}
