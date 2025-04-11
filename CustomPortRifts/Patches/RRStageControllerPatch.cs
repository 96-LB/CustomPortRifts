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
    public static float lastMiss = 0.0f;
    public static float lastVanillaMiss = 0.0f;

    [HarmonyPatch(nameof(P.UnpackScenePayload))]
    [HarmonyPostfix]
    public async static void UnpackScenePayload(
        ScenePayload currentScenePayload
    ) {
        lastMiss = -1.0f;
        lastVanillaMiss = -1.0f;
        if(
            currentScenePayload is not RRCustomTrackScenePayload payload
            || !Config.General.Enabled.Value
        ) {
            Portrait.Reset();
            return;
        }

        
        var dir = Path.GetDirectoryName(payload.GetBeatmapFileName());
        dir = Path.Combine(dir, "CustomPortRifts");

        // search for portraits
        if(!Directory.Exists(dir)) {
            Plugin.Log.LogInfo("No custom portrait folder found. Folder should be called 'CustomPortRifts' and be located in the same directory as the beatmap. Falling back to default.");
            return;
        }

        Portrait.Loading = true;

        // always load settings, even on level retry, since this is cheap
        var settingsFile = Path.Combine(dir, "config.json");
        if(File.Exists(settingsFile)) {
            Plugin.Log.LogInfo("Loading configuration file.");
            await Portrait.LoadSettings(settingsFile);
        } else {
            Plugin.Log.LogInfo("No configuration file found. File should be called 'config.json' and be located in the custom portraits directory. Using default settings.");
            Portrait.ResetSettings();
        }

        var usingPortraits = Config.Custom.Portraits.Value;
        usingPortraits &= !payload.IsPracticeMode || Config.PracticeMode.Portraits.Value;
        if(!usingPortraits) {
            // the above conditions should force a portrait reset
            Portrait.Reset();
        }

        // don't reload sprites if we're just retrying the same level
        string levelId = payload.GetLevelId();
        if(usingPortraits && levelId != Portrait.LevelId) {
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
                Plugin.Log.LogInfo("Custom portraits loaded successfully.");
            } else {
                Plugin.Log.LogInfo("No custom portraits were loaded.");
            }
        }

        var usingParticles = Config.Custom.Particles.Value;
        usingParticles &= !payload.IsPracticeMode || Config.PracticeMode.Particles.Value;
        if(usingParticles && levelId != Portrait.LevelId) {
            var particlesFile = Path.Combine(dir, "particles.png");
            if(File.Exists(particlesFile)) {
                Plugin.Log.LogInfo("Loading particles file.");
                await Portrait.LoadParticles(particlesFile);
            } else {
                Plugin.Log.LogInfo("No particles file found. File should be called 'particles.png' and be located in the custom portraits directory. Using default particles.");
            }
        }
        
        // TODO: latest edits lose out on the nice feature from before where toggling the setting midgame would reload portraits on next retry.
        // bring that back (probably requires separating out the particles)
        Portrait.LevelId = levelId;
        Portrait.Loading = false;
    }

    //Miss when hit by enemy
    [HarmonyPatch( nameof(P.HandleEnemyAttack) )]
    [HarmonyPrefix]
    public static void HandleEnemyAttack( P __instance ){
        Shared.RhythmEngine.FmodTimeCapsule fmodTimeCapsule = __instance.BeatmapPlayer.FmodTimeCapsule;
        lastMiss = fmodTimeCapsule.TrueBeatNumber;
    }

    //Miss when hit by overtapping as Coda
    [HarmonyPatch( nameof(P.HandleCodaErrantDamage) )]
    [HarmonyPrefix]
    public static void HandleCodaErrantDamage( P __instance ){
        Shared.RhythmEngine.FmodTimeCapsule fmodTimeCapsule = __instance.BeatmapPlayer.FmodTimeCapsule;
        lastMiss = fmodTimeCapsule.TrueBeatNumber;
    }

    [HarmonyPatch(nameof(P.InitializeBackgroundRoutine))]
    [HarmonyPostfix]
    public static void InitializeBackgroundRoutine(
        P __instance,
        ref IEnumerator __result
    ) {
        if(!Config.General.Enabled.Value) {
            return;
        }
        
        // since the original function is a coroutine, we need to wrap the output to properly postfix
        var original = __result;
        __result = Wrapper();

        IEnumerator Wrapper() {
            yield return original;

            IEnumerator TryLoad(string id, System.Action<RiftFXColorConfig> callback) {
                if(string.IsNullOrWhiteSpace(id)) {
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
                var settings = Portrait.Settings.vfx;
                var baseConfig = Object.Instantiate(__instance._riftFXConfig.CharacterRiftColorConfig);

                var usingColors = Config.Custom.Colors.Value;
                usingColors &= !__instance._isPracticeMode || Config.PracticeMode.Colors.Value;
                var usingParticles = Config.Custom.Particles.Value;
                usingParticles &= !__instance._isPracticeMode || Config.PracticeMode.Particles.Value;

                // load character defaults for background visuals
                if(__instance._riftCharacterFxConfigDatabase) {
                    var charConfig = baseConfig;

                    if(usingColors) {
                        yield return TryLoad(settings.character, x => charConfig = x);
                        baseConfig.BackgroundMaterial = charConfig.BackgroundMaterial;
                        baseConfig.CoreStartColor1 = charConfig.CoreStartColor1;
                        baseConfig.CoreStartColor2 = charConfig.CoreStartColor2;
                        baseConfig.CoreColorOverLifetime = charConfig.CoreColorOverLifetime;
                        baseConfig.RiftGlowColor = charConfig.RiftGlowColor;
                        baseConfig.StrobeColor1 = charConfig.StrobeColor1;
                        baseConfig.StrobeColor2 = charConfig.StrobeColor2;
                        baseConfig.SpeedlinesStartColor = charConfig.SpeedlinesStartColor;
                        baseConfig.SpeedlinesColorOverLifetime = charConfig.SpeedlinesColorOverLifetime;
                    }

                    if(usingParticles) {
                        yield return TryLoad(settings.particles.character ?? settings.character, x => charConfig = x);
                        baseConfig.CustomParticleColor1 = charConfig.CustomParticleColor1;
                        baseConfig.CustomParticleColor2 = charConfig.CustomParticleColor2;
                        baseConfig.CustomParticleColorOverLifetime = charConfig.CustomParticleColorOverLifetime;
                        baseConfig.CustomParticleMaterial = charConfig.CustomParticleMaterial;
                    }
                }

                if(usingColors) {
                    // set the cloud colors from the settings
                    var clouds = settings.clouds;
                    clouds.color1?.Pipe(x => baseConfig.CoreStartColor1 = x);
                    clouds.color2?.Pipe(x => baseConfig.CoreStartColor2 = x);
                    clouds.gradient?.Pipe(x => baseConfig.CoreColorOverLifetime = x);

                    // set the particle colors from the settings
                    var particles = settings.particles.color;
                    particles.color1?.Pipe(x => baseConfig.CustomParticleColor1 = x);
                    particles.color2?.Pipe(x => baseConfig.CustomParticleColor2 = x);
                    particles.gradient?.Pipe(x => baseConfig.CustomParticleColorOverLifetime = x);

                    // set the speedline colors from the settings
                    var speedlines = settings.speedlines;
                    speedlines.color1?.Pipe(x => baseConfig.SpeedlinesStartColor = x);
                    // TODO: we ignore color2 here. probably not the best design
                    speedlines.gradient?.Pipe(x => baseConfig.SpeedlinesColorOverLifetime = x);

                    // set the rift color from the settings
                    settings.rift?.Pipe(x => baseConfig.RiftGlowColor = x);

                    // TODO: figure out if the strobe colors are actually meaningful

                    // clone the background material so we don't make permanent edits
                    baseConfig.BackgroundMaterial = new(baseConfig.BackgroundMaterial);

                    // TODO: this part probably breaks if something like suzu (reaper?) is loaded
                    // set the background colors from the settings
                    var background = settings.background;
                    var mat = baseConfig.BackgroundMaterial;
                    background.color?.Pipe(x => mat.SetColor("_BottomColor", x));
                    background.highlightColor?.Pipe(x => mat.SetColor("_TopColor", x));
                    background.intensity?.Pipe(x => mat.SetFloat("_GradientIntensity", x));
                    background.intensity2?.Pipe(x => mat.SetFloat("_AdditiveIntensity", x));
                    background.rotation?.Pipe(x => mat.SetFloat("_RotateGradient", x));
                }

                if(usingParticles) {
                    // if a custom particle texture is provided, replace the default one
                    Portrait.Particles?.Pipe(x => {
                        baseConfig.CustomParticleMaterial = new(baseConfig.CustomParticleMaterial);
                        baseConfig.CustomParticleMaterial.SetTexture("_Texture2D", x);

                        // modify the texture sheet settings to match the number of particles
                        settings.particles.count?.Pipe(x => {
                            int dim = Mathf.CeilToInt(Mathf.Sqrt(Mathf.Max(1, x)));
                            __instance?._rhythmRiftBackgroundFx?._customCharacterParticles?.textureSheetAnimation.Pipe(y => {
                                y.numTilesX = dim;
                                y.numTilesY = dim;
                                y.startFrame = new(0, x / Mathf.Pow(dim, 2));
                            });
                        });
                    });

                    // particle rotation can be modified whether or not a custom texture is provided
                    settings.particles.rotation?.Pipe(x => {
                        baseConfig.HasCustomRotation = x != 0;
                        baseConfig.CustomParticleRotation = x;
                    });
                }
                
                // update the configuration with all our new values
                __instance._riftFXConfig.CharacterRiftColorConfig = baseConfig;
                __instance._rhythmRiftBackgroundFx.SetConfig(__instance._riftFXConfig, __instance.BeatmapPlayer, backgroundDetailLevel == BackgroundDetailLevel.NoBackground);
            }
        }
    }
}
