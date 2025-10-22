using HarmonyLib;
using Shared.SceneLoading;
using System.Collections;

namespace CustomPortRifts.Patches;


[HarmonyPatch(typeof(SceneLoadingController))]
public static class SceneLoadingPatch {
    [HarmonyPatch(nameof(SceneLoadingController.GoToSceneRoutine))]
    [HarmonyPostfix]
    public static void GoToSceneRoutine(SceneLoadingController __instance, ref IEnumerator __result, bool isReloading) {
        // since the original function is a coroutine, we need to wrap the output to properly postfix
        var original = __result;
        __result = Wrapper();

        IEnumerator Wrapper() {
            if(!isReloading) {
                // clear the portrait cache
                PortraitViewState.Portraits.Clear();
            }
            yield return original;
        }
    }
}
