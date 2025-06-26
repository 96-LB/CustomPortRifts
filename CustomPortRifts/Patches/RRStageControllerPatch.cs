using HarmonyLib;
using RhythmRift;
using Shared.SceneLoading.Payloads;

namespace CustomPortRifts.Patches;


[HarmonyPatch(typeof(RRStageController))]
public static class RRStageControllerPatch {
    [HarmonyPatch(nameof(RRStageController.UnpackScenePayload))]
    [HarmonyPostfix]
    public static void UnpackScenePayload(RRStageController __instance, ScenePayload currentScenePayload) {
        var portrait = PortraitState.Of(__instance._portraitUiController);
        portrait.LevelId = currentScenePayload.GetLevelId();
        if(currentScenePayload is RhythmRiftScenePayload riftScene) {
            portrait.TrackName = riftScene.TrackName;
        }
        Plugin.Log.LogMessage(portrait.LevelId);
    }
}
