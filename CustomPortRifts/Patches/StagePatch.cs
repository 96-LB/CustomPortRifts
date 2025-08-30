using HarmonyLib;
using RhythmRift;
using Shared.SceneLoading.Payloads;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections;
using System.IO;

namespace CustomPortRifts.Patches;


public class StageState : State<RRStageController, StageState> {
    public string BasePortraitPath { get; set; } = "";
}

[HarmonyPatch(typeof(RRStageController))]
public static class StagePatch {
    [HarmonyPatch(nameof(RRStageController.UnpackScenePayload))]
    [HarmonyPostfix]
    public static void UnpackScenePayload(RRStageController __instance, ScenePayload currentScenePayload) {
        var portrait = PortraitState.Of(__instance._portraitUiController);
        portrait.LevelId = currentScenePayload.GetLevelId();
        if(currentScenePayload is RhythmRiftScenePayload rrPayload && rrPayload.TrackMetadata is LocalTrackMetadata metadata) {
            var state = StageState.Of(__instance);
            state.BasePortraitPath = Path.Combine(metadata.BasePath, LocalTrackPortrait.DefaultDirectory);
        }
    }

    [HarmonyPatch(nameof(RRStageController.CounterpartPortraitOverride), MethodType.Getter)]
    [HarmonyPostfix]
    public static void CounterpartPortraitOverride(RRStageController __instance, ref string? __result) {
        StageScenePayload stageScenePayload = __instance._stageScenePayload;
        if(stageScenePayload != null) {
            if(Config.ExtraModes.DisableBeastmaster && __result == __instance.BeastmasterPortraitCharacterId && !__instance._isCalibrationTest) {
                __result = null;
            }

            if(Config.ExtraModes.DisableShopkeeper && __result == __instance.ShopkeeperPortraitCharacterId) {
                __result = null;
            }

            if(Config.ExtraModes.DisableCoda && __result == __instance.CodaPortraitCharacterId) {
                __result = null;
            }
        }
    }

    [HarmonyPatch(nameof(RRStageController.StageInitialize))]
    [HarmonyPostfix]
    public static void StageInitialize(RRStageController __instance, ref IEnumerator __result) {
        // since the original function is a coroutine, we need to wrap the output to properly postfix
        var original = __result;
        __result = Wrapper();

        IEnumerator Wrapper() {
            Plugin.Log.LogMessage("StageInitialize Postfix");
            yield return original;
            
            var state = StageState.Of(__instance);

            var ui = __instance._portraitUiController;
            var counterpartAnimator = ui._counterpartPortraitParent.GetComponentInChildren<DataDrivenAnimator>();
            var counterpart = counterpartAnimator ? AnimatorState.Of(counterpartAnimator) : null;
            var heroAnimator = ui._heroPortraitParent.GetComponentInChildren<DataDrivenAnimator>();
            var hero = heroAnimator ? AnimatorState.Of(heroAnimator) : null;

            var beatmapPlayer = BeatmapState.Of(__instance.BeatmapPlayer);
            beatmapPlayer.Counterpart = counterpart;
            beatmapPlayer.Hero = hero;
            
            foreach(var beatmap in __instance._beatmaps) {
                foreach(var beatmapEvent in beatmap.BeatmapEvents) {
                    Plugin.Log.LogInfo(beatmapEvent.type);
                    if(beatmapEvent.type == Constants.EVENT_SETPORTRAIT) { // TODO: abstract this
                        Plugin.Log.LogMessage("Loading custom portrait from beatmap event...");
                        var name = beatmapEvent.GetFirstEventDataAsString(Constants.KEY_PORTRAITNAME);
                        if(name == null) {
                            Plugin.Log.LogWarning("Portrait name was null, skipping...");
                            continue;
                        }
                        var isHero = beatmapEvent.GetFirstEventDataAsBool(Constants.KEY_ISHERO) ?? false;
                        var animator = isHero ? hero : counterpart;
                        animator?.AddPortrait(state.BasePortraitPath, name);
                    }
                }
            }
        }
    }       
}
