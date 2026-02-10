using HarmonyLib;
using RhythmRift;
using RiftOfTheNecroManager;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections;
using System.IO;
using UnityEngine;

namespace CustomPortRifts.Patches;


public class PortraitState : State<RRPortraitUiController, PortraitState> {
    public string LevelId { get; set; } = "";
}


[HarmonyPatch(typeof(RRPortraitUiController))]
public static class PortraitControllerPatch {
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
        characterId = characterId switch {
            "Cadence" or "Cadence_Supporter" when Config.Reskins.CryptCadence => "CadenceCrypt",
            "NecrodancerCloak" when Config.Reskins.CryptNecrodancer => "NecrodancerCrypt",
            "NecrodancerCloak" when Config.Reskins.Necroburger => "NecrodancerBurger",
            _ => characterId
        };

        if(!Config.General.TrackOverrides && !Config.General.CharacterOverrides) {
            return;
        }

        var basePath = PluginData.DataPath;
        if(!FileUtils.IsDirectory(basePath)) {
            Log.Info($"No custom portrait directory found. The folder should be named '{PluginData.Name}' and located in the same directory as the game executable. No custom portraits will be loaded.");
            return;
        }

        LocalTrackPortrait? portrait = null;
        var state = PortraitState.Of(__instance);
        var dirName = isHeroPortrait ? "Hero" : "Counterpart";
        var portraitType = "Custom" + dirName;

        // load the track-specific override
        if(Config.General.TrackOverrides) {
            Log.Info($"Loading custom {dirName.ToLowerInvariant()} portrait...");

            var trackPath = Path.Combine(basePath, "Tracks");
            if(!FileUtils.IsDirectory(trackPath)) {
                Log.Info($"No track override directory found. The folder should be named 'Tracks' and located in '{PluginData.Name}'. No track overrides will be applied.");
            } else {
                trackPath = Path.Combine(trackPath, state.LevelId, dirName);
                if(!FileUtils.IsDirectory(trackPath)) {
                    Log.Info($"No track directory found using level ID. The folder should be named '{state.LevelId}/{dirName}' and located in '{PluginData.Name}/Tracks'. No track overrides will be applied.");
                } else {
                    var trackPortrait = LocalTrackPortrait.TryLoadCustomPortrait(trackPath, portraitType);
                    if(trackPortrait == null) {
                        Log.Warning($"Failed to load custom {dirName.ToLowerInvariant()} portrait for {state.LevelId} from track override.");
                    } else {
                        portrait = trackPortrait;
                        Log.Info($"Loaded custom {dirName.ToLowerInvariant()} portrait for {state.LevelId} from track override.");
                    }
                }
            }
        }

        // load the character-specific override
        if(Config.General.CharacterOverrides) {
            var id = portrait?.PortraitId ?? characterId;
            Log.Info($"Loading custom portrait for {id}...");

            var charPath = Path.Combine(basePath, "Characters");
            if(!FileUtils.IsDirectory(charPath)) {
                Log.Info($"No character override directory found. The folder should be named 'Characters' and located in '{PluginData.Name}'. No character overrides will be applied.");
            } else {
                charPath = Path.Combine(charPath, id);
                if(!FileUtils.IsDirectory(charPath)) {
                    Log.Info($"No character directory found using character ID. The folder should be named '{id}' and located in '{PluginData.Name}/Characters'. No character overrides will be applied.");
                } else {
                    var charPortrait = LocalTrackPortrait.TryLoadCustomPortrait(charPath, portraitType);
                    if(charPortrait == null) {
                        Log.Warning($"Failed to load custom portrait for {id} from character override.");
                    } else {
                        portrait = charPortrait;
                        Log.Info($"Loaded custom portrait for {id} from character override.");
                    }
                }
            }
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
