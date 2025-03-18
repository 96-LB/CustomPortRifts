using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using HarmonyLib;
using RhythmRift;
using Shared.SceneLoading.Payloads;
using UnityEngine;

namespace CustomPortRifts.Patches;


using P = RRStageController;

[HarmonyPatch(typeof(P), "UnpackScenePayload")]
internal static class RRStageControllerPatch {
    public async static void Postfix(
        ScenePayload currentScenePayload
    ) {
        CustomPortraits.Enabled = Config.CustomPortraits.Enabled.Value;
        if(!CustomPortraits.Enabled || currentScenePayload is not RRCustomTrackScenePayload payload) {
            CustomPortraits.Reset();
            return;
        }
        
        string levelId = payload.GetLevelId();
        if(levelId == CustomPortraits.LevelId) {
            // don't reload sprites if we're just retrying the same level
            return;
        }
        
        CustomPortraits.Reset();

        var dir = Path.GetDirectoryName(payload.GetBeatmapFileName());
        dir = Path.Combine(dir, "CustomPortRifts");
        if(!Directory.Exists(dir)) {
            Plugin.Log.LogInfo("No custom portrait folder found. Folder should be called 'CustomPortRifts' and be located in the same directory as the beatmap. Falling back to default.");
            return;
        }

        async Task<Sprite[]> LoadSprites(string dirName) {
            var fullDir = Path.Combine(dir, dirName);
            List<Sprite> sprites = [];
            if(Directory.Exists(fullDir)) {
                var files = Directory.GetFiles(fullDir, "*.png");
                Array.Sort(files);
                foreach(var file in files) {
                    try {
                        var bytes = await File.ReadAllBytesAsync(file);
                        var texture = new Texture2D(1, 1);
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

        var normalSprites = await LoadSprites("Normal");
        var poorlySprites = await LoadSprites("DoingPoorly");
        var wellSprites = await LoadSprites("DoingWell");
        var vibePowerSprites = await LoadSprites("VibePower");

        normalSprites ??= wellSprites ?? poorlySprites ?? vibePowerSprites;
        wellSprites ??= normalSprites;
        poorlySprites ??= normalSprites;
        vibePowerSprites ??= wellSprites;

        if(normalSprites == null) {
            Plugin.Log.LogInfo("No custom portrait sprites found, though the folder exists. Sprites should be .png format and located in subfolders called 'Normal', 'DoingPoorly', or 'DoingWell'. Falling back to default.");
            return;
        }

        CustomPortraits.SetSprites(levelId, normalSprites, poorlySprites, wellSprites, vibePowerSprites);
    }
}
