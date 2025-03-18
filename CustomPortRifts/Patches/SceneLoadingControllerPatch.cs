using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Shared.SceneLoading;
using Shared.SceneLoading.Payloads;

namespace CustomPortRifts.Patches;


using P = SceneLoadingController;

[HarmonyPatch(typeof(P), "LoadInNewScene")]
internal static class SceneLoadingControllerPatch {
    public static void Prefix() {
        // ensure we unload custom portraits when exiting a track
        if(SceneLoadData.TryGetCurrentPayload(out var rawPayload) && rawPayload is not RRCustomTrackScenePayload) {
            CustomPortraits.Reset();
        }
    }
}
