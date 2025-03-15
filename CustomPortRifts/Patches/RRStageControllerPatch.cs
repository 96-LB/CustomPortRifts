using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using RhythmRift;
using Shared.SceneLoading.Payloads;
using UnityEngine;

namespace CustomPortRifts.Patches;


using P = RRStageController;

[HarmonyPatch(typeof(P), "UnpackScenePayload")]
internal static class RRStageControllerPatch {
    public static void Postfix(
        ScenePayload currentScenePayload
    ) {
        BeatmapAnimatorControllerPatch.performanceLevel = RRPerformanceLevel.Normal;
        BeatmapAnimatorControllerPatch.normalSprites = null;
        BeatmapAnimatorControllerPatch.wellSprites = null;
        BeatmapAnimatorControllerPatch.poorlySprites = null;
        BeatmapAnimatorControllerPatch.vibePowerSprites = null;

        if(currentScenePayload is not RRCustomTrackScenePayload payload) {
            return;
        }

        var dir = Path.GetDirectoryName(payload.GetBeatmapFileName());
        dir = Path.Combine(dir, "CustomPortRifts");
        if(!Directory.Exists(dir)) {
            Plugin.Log.LogInfo("No custom portrait folder found. Folder should be called 'CustomPortRifts' and be located in the same directory as the beatmap. Falling back to default.");
            return;
        }

        Sprite[] LoadSprites(string dirName) {
            var fullDir = Path.Combine(dir, dirName);
            List<Sprite> sprites = [];
            if(Directory.Exists(fullDir)) {
                var files = Directory.GetFiles(fullDir, "*.png");
                Array.Sort(files);
                foreach(var file in files) {
                    var bytes = File.ReadAllBytes(file);
                    var texture = new Texture2D(1, 1);
                    try {
                        // TODO: this is slow! consider handling this asynchronously to avoid game freeze
                        // TODO: loading this on every reload is really bad
                        texture.LoadImage(bytes);
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        sprites.Add(sprite);
                    } catch(Exception e) {
                        Plugin.Log.LogError($"Failed to load image {file}: {e}");
                    }
                }
            }
            return sprites.Count > 0 ? [..sprites] : null;
        }

        var normalSprites = LoadSprites("Normal");
        var poorlySprites = LoadSprites("DoingPoorly");
        var wellSprites = LoadSprites("DoingWell");
        var vibePowerSprites = LoadSprites("VibePower");

        normalSprites ??= wellSprites ?? poorlySprites ?? vibePowerSprites;
        wellSprites ??= normalSprites;
        poorlySprites ??= normalSprites;
        vibePowerSprites ??= wellSprites;

        if(normalSprites == null) {
            Plugin.Log.LogInfo("No custom portrait sprites found, though the folder exists. Sprites should be .png format and located in subfolders called 'Normal', 'DoingPoorly', or 'DoingWell'. Falling back to default.");
            return;
        }

        BeatmapAnimatorControllerPatch.normalSprites = normalSprites;
        BeatmapAnimatorControllerPatch.wellSprites = wellSprites;
        BeatmapAnimatorControllerPatch.poorlySprites = poorlySprites;
        BeatmapAnimatorControllerPatch.vibePowerSprites = vibePowerSprites;
    }
}
