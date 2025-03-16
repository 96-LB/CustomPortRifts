using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using Shared.SceneLoading;
using Shared.SceneLoading.Payloads;
using UnityEngine;
using UnityEngine.Networking;

namespace CustomPortRifts.Patches;


using P = SceneLoadingController;

[HarmonyPatch(typeof(P), "LoadInNewScene")]
internal static class SceneLoadingControllerPatch {
    public static void Prefix() {
        if(SceneLoadData.TryGetCurrentPayload(out var rawPayload) && rawPayload is not RRCustomTrackScenePayload) {
            CustomPortraits.Reset();
        }
    }
}
