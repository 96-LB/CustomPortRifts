﻿using System;
using System.IO;
using HarmonyLib;
using RhythmRift;
using Shared.PlayerData;
using Shared;
using Shared.SceneLoading.Payloads;
using UnityEngine;
using System.Collections;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;

namespace CustomPortRifts.Patches;


using P = RRStageController;

[HarmonyPatch(typeof(P))]
internal static class RRStageControllerPatch {
    [HarmonyPatch(nameof(P.UnpackScenePayload))]
    [HarmonyPostfix]
    public async static void UnpackScenePayload(
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
            return; // TODO: we should probably load settings still
        }
        
        Portrait.Reset();

        var dir = Path.GetDirectoryName(payload.GetBeatmapFileName());
        dir = Path.Combine(dir, "CustomPortRifts");

        // search for portraits
        if(!Directory.Exists(dir)) {
            Plugin.Log.LogInfo("No custom portrait folder found. Folder should be called 'CustomPortRifts' and be located in the same directory as the beatmap. Falling back to default.");
            return;
        }

        Portrait.Loading = true;

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

        if(!Portrait.Hero.HasSprites && !Portrait.Counterpart.HasSprites) {
            Plugin.Log.LogInfo("No custom portraits were loaded.");
            Portrait.Loading = false;
            return;
        }

        Portrait.LevelId = levelId;
        Plugin.Log.LogInfo("Custom portraits loaded successfully.");
        
        // search for settings
        var settingsFile = Path.Combine(dir, "config.json");
        if(File.Exists(settingsFile)) {
            Plugin.Log.LogInfo("Loading configuration file.");
            await Portrait.LoadSettings(settingsFile);
        } else {
            Plugin.Log.LogInfo("No configuration file found. File should be called 'config.json' and be located in the custom portraits directory. Using default settings.");
        }

        Portrait.Loading = false;
    }

    [HarmonyPatch(nameof(P.InitializeBackgroundRoutine))]
    [HarmonyPostfix]
    public static void InitializeBackgroundRoutine(
        P __instance,
        ref IEnumerator __result
    ) {
        // since the original function is a coroutine, we need to wrap the output to properly postfix
        var original = __result;
        __result = Wrapper();

        IEnumerator Wrapper() {
            yield return original;

            IEnumerator TryLoad(string id, Action<RiftFXColorConfig> callback) {
                if(string.IsNullOrWhiteSpace(id)) {
                    yield break;
                }

                var characterFxConfigRef = __instance._riftCharacterFxConfigDatabase.GetConfig(id);
                if(characterFxConfigRef != null && characterFxConfigRef.RuntimeKeyIsValid()) {
                    var handle = Addressables.LoadAssetAsync<RiftFXColorConfig>(characterFxConfigRef);
                    if(handle.Status == AsyncOperationStatus.Succeeded) {
                        Plugin.Log.LogInfo($"Loaded character effect configuration for '{id}'.");
                        __instance._assetRefsToCleanUpOnDestroy.Add(characterFxConfigRef);
                        callback(handle.Result);
                        DebugUtil.Dump(handle.Result);
                    } else {
                        Plugin.Log.LogError($"Failed to load character effect configuration for '{id}'.");
                        characterFxConfigRef.ReleaseAsset();
                    }
                } else {
                    Plugin.Log.LogError($"Unknown character effect configuration '{id}'.");
                }
            }

            var backgroundDetailLevel = PlayerSaveController.Instance.GetBackgroundDetailLevel();
            if(__instance._rhythmRiftBackgroundFx && __instance._riftFXConfig) {
                var settings = Portrait.Settings.background;
                var baseConfig = __instance._riftFXConfig.CharacterRiftColorConfig;
                var colorConfig = baseConfig;
                var particleConfig = baseConfig;

                if(__instance._riftCharacterFxConfigDatabase) {
                    yield return TryLoad(settings.color, config => colorConfig = config);
                    yield return TryLoad(settings.particles, config => particleConfig = config);
                }

                baseConfig.BackgroundMaterial = colorConfig.BackgroundMaterial;
                baseConfig.CoreStartColor1 = colorConfig.CoreStartColor1;
                baseConfig.CoreStartColor2 = colorConfig.CoreStartColor2;
                baseConfig.CoreColorOverLifetime = colorConfig.CoreColorOverLifetime;
                baseConfig.RiftGlowColor = colorConfig.RiftGlowColor;
                baseConfig.StrobeColor1 = colorConfig.StrobeColor1;
                baseConfig.StrobeColor2 = colorConfig.StrobeColor2;
                baseConfig.SpeedlinesStartColor = colorConfig.SpeedlinesStartColor;
                baseConfig.SpeedlinesColorOverLifetime = colorConfig.SpeedlinesColorOverLifetime;

                baseConfig.CustomParticleColor1 = particleConfig.CustomParticleColor1;
                baseConfig.CustomParticleColor2 = particleConfig.CustomParticleColor2;
                baseConfig.CustomParticleColorOverLifetime = particleConfig.CustomParticleColorOverLifetime;
                baseConfig.CustomParticleMaterial = particleConfig.CustomParticleMaterial;

                if(settings.rotation.HasValue && float.IsNormal(settings.rotation.Value)) {
                    baseConfig.HasCustomRotation = settings.rotation.Value != 0;
                    baseConfig.CustomParticleRotation = settings.rotation.Value;
                }

                __instance._riftFXConfig.CharacterRiftColorConfig = baseConfig;
                DebugUtil.Dump(colorConfig);
                DebugUtil.Dump(__instance._riftFXConfig);
                __instance._rhythmRiftBackgroundFx.SetConfig(__instance._riftFXConfig, __instance.BeatmapPlayer, backgroundDetailLevel == BackgroundDetailLevel.NoBackground);
            }
        }
    }
}
