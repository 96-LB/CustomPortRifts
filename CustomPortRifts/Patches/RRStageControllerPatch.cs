using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using RhythmRift;
using Shared.SceneLoading.Payloads;

namespace CustomPortRifts.Patches;


using P = RRStageController;

[HarmonyPatch(typeof(P), "UnpackScenePayload")]
internal static class RRStageControllerPatch {
    public async static void Postfix(
        ScenePayload currentScenePayload
    ) {
        Portrait.Enabled = Config.CustomPortraits.Enabled.Value;
        if(!Portrait.Enabled || currentScenePayload is not RRCustomTrackScenePayload payload) {
            Portrait.Reset();
            return;
        }
        
        string levelId = payload.GetLevelId();
        if(levelId == Portrait.LevelId) {
            // don't reload sprites if we're just retrying the same level
            return;
        }
        
        Portrait.Reset();
        Portrait.Loading = true;

        var dir = Path.GetDirectoryName(payload.GetBeatmapFileName());
        dir = Path.Combine(dir, "CustomPortRifts");
        if(!Directory.Exists(dir)) {
            Plugin.Log.LogInfo("No custom portrait folder found. Folder should be called 'CustomPortRifts' and be located in the same directory as the beatmap. Falling back to default.");
            return;
        }

        foreach(var (subdir, portrait) in new[] { ("Counterpart", Portrait.Counterpart), ("Hero", Portrait.Hero) }) {
            var fullDir = Path.Combine(dir, subdir);
            if(Directory.Exists(fullDir)) {
                await portrait.LoadSprites(fullDir);
                if(!portrait.HasSprites) {
                    Plugin.Log.LogInfo($"No {subdir.ToLower()} portrait sprites found, though the folder exists. Sprites should be .png format and located in subfolders called 'Normal', 'DoingPoorly', or 'DoingWell'. Falling back to default.");
                }
            } else {
                Plugin.Log.LogInfo($"No {subdir.ToLower()} folder found. Folder should be called '{subdir}' and located in the custom portraits directory. Falling back to default.");
            }
        }

        if(Portrait.Hero.HasSprites || Portrait.Counterpart.HasSprites) {
            Portrait.LevelId = levelId;
            Plugin.Log.LogInfo("Custom portraits loaded successfully.");
        }
        Portrait.Loading = false;
    }
}
