using CustomPortRifts.BeatmapEvents;
using HarmonyLib;
using RhythmRift;
using Shared.SceneLoading.Payloads;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

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
            CustomEvent.FlagAllForProcessing(__instance._beatmaps);

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

            var tasks = new List<IEnumerator>();
            
            foreach(var setPortraitEvent in CustomEvent.Enumerate<SetPortraitEvent>(__instance._beatmaps)) {
                var animator = setPortraitEvent.IsHero ? hero : counterpart;
                animator?.AddPortrait(state.BasePortraitPath, setPortraitEvent.Name)
                    .Pipe(AsyncUtils.WaitForTask)
                    .Pipe(tasks.Add);
            }

            foreach(var task in tasks) {
                yield return task;
            }
        }
    }       
}
