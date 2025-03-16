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
    public static void Prefix(
        P __instance,
        ref Action onSceneLoadedCallback
    ) {
        if(!SceneLoadData.TryGetCurrentPayload(out var rawPayload)) {
            return;
        }

        CustomPortraits.Enabled = Config.CustomPortraits.Enabled.Value;
        if(!CustomPortraits.Enabled || rawPayload is not RRCustomTrackScenePayload payload) {
            CustomPortraits.Reset();
            return;
        }

        string levelId = payload.GetLevelId();
        if(levelId == CustomPortraits.LevelId) {
            // don't reload sprites if we're just retrying the same level
            return;
        }

        CustomPortraits.Reset();
        onSceneLoadedCallback += () => __instance.StartCoroutine(Coroutine());

        IEnumerator Coroutine() {
            var dir = Path.GetDirectoryName(payload.GetBeatmapFileName());
            dir = Path.Combine(dir, "CustomPortRifts");
            if(!Directory.Exists(dir)) {
                Plugin.Log.LogInfo("No custom portrait folder found. Folder should be called 'CustomPortRifts' and be located in the same directory as the beatmap. Falling back to default.");
                yield break;
            }

            var allSprites = new Sprite[4][];
            string[] dirs = ["Normal", "DoingPoorly", "DoingWell", "VibePower"];

            for(int i = 0; i < dirs.Length; i++) {
                var fullDir = Path.Combine(dir, dirs[i]);
                List<Sprite> spriteList = [];
                if(Directory.Exists(fullDir)) {
                    var files = Directory.GetFiles(fullDir, "*.png");
                    Array.Sort(files);
                    foreach(var file in files) {
                        Plugin.Log.LogInfo("attempting to read " + file);
                        var request = UnityWebRequestTexture.GetTexture(file);
                        yield return request.SendWebRequest();
                        if(request.result != UnityWebRequest.Result.Success) {
                            Plugin.Log.LogError($"Failed to load image {file}: {request.error}");
                            continue;
                        }
                        var texture = DownloadHandlerTexture.GetContent(request);
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        spriteList.Add(sprite);
                    }
                }
                allSprites[i] = spriteList.Count > 0 ? [.. spriteList] : null;
            }

            var normalSprites = allSprites[0];
            var poorlySprites = allSprites[1];
            var wellSprites = allSprites[2];
            var vibePowerSprites = allSprites[3];

            normalSprites ??= wellSprites ?? poorlySprites ?? vibePowerSprites;
            wellSprites ??= normalSprites;
            poorlySprites ??= normalSprites;
            vibePowerSprites ??= wellSprites;

            if(normalSprites == null) {
                Plugin.Log.LogInfo("No custom portrait sprites found, though the folder exists. Sprites should be .png format and located in subfolders called 'Normal', 'DoingPoorly', or 'DoingWell'. Falling back to default.");
                yield break;
            }

            CustomPortraits.SetSprites(levelId, normalSprites, wellSprites, poorlySprites, vibePowerSprites);
        }
    }
}
