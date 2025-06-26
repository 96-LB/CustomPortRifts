using HarmonyLib;
using RhythmRift;
using Shared.SceneLoading.Payloads;

namespace CustomPortRifts.Patches;


[HarmonyPatch(typeof(RRStageController))]
public static class RRStageControllerPatch {
    public static string levelId = "";
    public static string trackName = "";

    [HarmonyPatch(nameof(RRStageController.UnpackScenePayload))]
    [HarmonyPostfix]
    public static void UnpackScenePayload(ScenePayload currentScenePayload) {
        levelId = currentScenePayload.GetLevelId();
        if(currentScenePayload is RhythmRiftScenePayload riftScene) {
            trackName = riftScene.TrackName;
        }
        Plugin.Log.LogMessage(levelId);
    }
}
