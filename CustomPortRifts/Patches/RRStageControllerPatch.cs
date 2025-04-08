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
using UnityEngine.Rendering;

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

            IEnumerator TryLoad(bool config, string id, System.Action<RiftFXColorConfig> callback) {
                if(!config || string.IsNullOrWhiteSpace(id)) {
                    yield break;
                }

                var characterFxConfigRef = __instance._riftCharacterFxConfigDatabase.GetConfig(id);
                if(characterFxConfigRef != null && characterFxConfigRef.RuntimeKeyIsValid()) {
                    var handle = Addressables.LoadAssetAsync<RiftFXColorConfig>(characterFxConfigRef);
                    yield return handle;
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
                var baseConfig = Object.Instantiate(__instance._riftFXConfig.CharacterRiftColorConfig);
                var colorConfig = baseConfig;
                var particleConfig = baseConfig;

                if(__instance._riftCharacterFxConfigDatabase) {
                    yield return TryLoad(Config.CustomBackgrounds.Colors.Value, settings.character, config => colorConfig = particleConfig = config);
                    yield return TryLoad(Config.CustomBackgrounds.Particles.Value, settings.particles.character, config => particleConfig = config);
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

                var color1 = settings.color1;
                if(color1.HasValue) {
                    // TODO: should make a gradient converter
                    var gradient = new Gradient();
                    gradient.SetKeys([new(color1.Value, 0)], [new(color1.Value.a, 0)]);
                    baseConfig.CoreStartColor1 = gradient;
                }

                var color2 = settings.color2;
                if(color2.HasValue) {
                    var gradient = new Gradient();
                    gradient.SetKeys([new(color2.Value, 0)], [new(color2.Value.a, 0)]);
                    baseConfig.CoreStartColor2 = gradient;
                }

                var colorOverTime = settings.colorOverTime;
                if(colorOverTime.HasValue) {
                    var gradient = new Gradient();
                    gradient.SetKeys([new(colorOverTime.Value, 0)], [new(colorOverTime.Value.a, 0)]);
                    baseConfig.CoreColorOverLifetime = gradient;
                }

                var particleColor1 = settings.particles.color1;
                if(particleColor1.HasValue) {
                    var gradient = new Gradient();
                    gradient.SetKeys([new(particleColor1.Value, 0)], [new(particleColor1.Value.a, 0)]);
                    baseConfig.CustomParticleColor1 = gradient;
                }

                var particleColor2 = settings.particles.color2;
                if(particleColor2.HasValue) {
                    var gradient = new Gradient();
                    gradient.SetKeys([new(particleColor2.Value, 0)], [new(particleColor2.Value.a, 0)]);
                    baseConfig.CustomParticleColor2 = gradient;
                }

                var particleColorOverTime = settings.particles.colorOverTime;
                if(particleColorOverTime.HasValue) {
                    var gradient = new Gradient();
                    gradient.SetKeys([new(particleColorOverTime.Value, 0)], [new(particleColorOverTime.Value.a, 0)]);
                    baseConfig.CustomParticleColorOverLifetime = gradient;
                }

                var glow = settings.glow;
                if(glow.HasValue) {
                    baseConfig.RiftGlowColor = glow.Value;
                }

                Plugin.Log.LogWarning("!!!!!!!!!!!!!!!!!!!!!");
                for(int i = 0; i < baseConfig.BackgroundMaterial.shader.GetPropertyCount(); i++) {
                    // TODO: list all the shader properties
                    var propertyName = baseConfig.BackgroundMaterial.shader.GetPropertyName(i);
                    var propertyType = baseConfig.BackgroundMaterial.shader.GetPropertyType(i);
                    object value = propertyType switch {
                        ShaderPropertyType.Color => baseConfig.BackgroundMaterial.GetColor(propertyName),
                        ShaderPropertyType.Vector => baseConfig.BackgroundMaterial.GetVector(propertyName),
                        ShaderPropertyType.Float => baseConfig.BackgroundMaterial.GetFloat(propertyName),
                        ShaderPropertyType.Range => baseConfig.BackgroundMaterial.GetFloat(propertyName),
                        ShaderPropertyType.Texture => baseConfig.BackgroundMaterial.GetTexture(propertyName),
                        _ => null
                    };
                    Plugin.Log.LogMessage($"{propertyName} {propertyType} {value}");
                }
                Plugin.Log.LogWarning("!!!!!!!!!!!!!!!!!!!!");

                // TODO: these break if something like suzu is loaded
                baseConfig.BackgroundMaterial = new(baseConfig.BackgroundMaterial);
                var bgColor = settings.bgColor1; // TODO: change this
                if(bgColor.HasValue) {
                    baseConfig.BackgroundMaterial.SetColor("_TopColor", bgColor.Value);
                }
                if(settings.intensity.HasValue) {
                    baseConfig.BackgroundMaterial.SetFloat("_GradientIntensity", settings.intensity.Value);
                }
                if(settings.additiveintensity.HasValue) {
                    baseConfig.BackgroundMaterial.SetFloat("_AdditiveIntensity", settings.additiveintensity.Value);
                }
                if(settings.bgColor2.HasValue) {
                    baseConfig.BackgroundMaterial.SetColor("_BottomColor", settings.bgColor2.Value);
                }
                if(settings.rotategradient.HasValue) {
                    baseConfig.BackgroundMaterial.SetFloat("_RotateGradient", settings.rotategradient.Value);
                }

                var rotation = settings.particles.rotation;
                if(rotation.HasValue) {
                    baseConfig.HasCustomRotation = rotation.Value != 0;
                    baseConfig.CustomParticleRotation = rotation.Value;
                }

                __instance._riftFXConfig.CharacterRiftColorConfig = baseConfig;
                //DebugUtil.Dump(colorConfig);
                //DebugUtil.Dump(__instance._riftFXConfig);
                __instance._rhythmRiftBackgroundFx.SetConfig(__instance._riftFXConfig, __instance.BeatmapPlayer, backgroundDetailLevel == BackgroundDetailLevel.NoBackground);
            }
        }
    }
}
