using CustomPortRifts.BeatmapEvents;
using CustomPortRifts.Transitions;
using HarmonyLib;
using Newtonsoft.Json;
using RhythmRift;
using Shared;
using Shared.SceneLoading.Payloads;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomPortRifts.Patches;


public class StageState : State<RRStageController, StageState> {
    public const string CUSTOMPORTRIFTS = "CustomPortRifts";
    public const string VFX_JSON = "vfx.json";

    public string BasePath { get; set; } = "";
    public string BasePortraitPath => Path.Combine(BasePath, CUSTOMPORTRIFTS);
    public string VfxPath => Path.Combine(BasePortraitPath, VFX_JSON);
    public Dictionary<string, VfxData> VfxData { get; } = [];

    public TransitionManager<RiftFXColorConfig> Transition { get; } = new();

    public Texture2D? TryLoadParticleTexture(LocalTrackVfxConfig config) {
        if(config.CustomParticleImagePath != null) {
            var bytes = FileUtils.ReadBytes(config.CustomParticleImagePath);
            if(bytes != null) {
                var texture = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
                if(texture.LoadImage(bytes)) {
                    return texture;
                }
            }
        }
        return null;
    }

    public bool TryLoadVfxConfigs() {
        if(!FileUtils.Exists(VfxPath)) {
            Plugin.Log.LogInfo($"No custom {VFX_JSON} file found in {CUSTOMPORTRIFTS} directory. No extra VFX will be loaded.");
            return false;
        }
        
        string? text;
        try {
            text = FileUtils.ReadCompressedString(VfxPath);
        } catch(JsonReaderException e) {
            Plugin.Log.LogWarning($"Failed to parse custom {VFX_JSON} file: {e.Message}");
            return false;
        }

        if(text != null) {
            var configs = JsonConvert.DeserializeObject<Dictionary<string, LocalTrackVfxConfig>>(text);
            if(configs != null) {
                VfxData.Clear();
                foreach(var (key, config) in configs) {
                    if(config.CustomParticleImagePath != null) {
                        config.CustomParticleImagePath = Path.Combine(BasePath, config.CustomParticleImagePath);
                    }
                    var particleTexture = TryLoadParticleTexture(config);
                    VfxData[key] = new(config, particleTexture);
                }
                Plugin.Log.LogInfo($"Loaded {VfxData.Count} VFX configs.");
                return true;
            }
        }
        Plugin.Log.LogWarning($"Failed to load VFX configs from {VfxPath}."); // TODO: more descriptive guard clauses/errors?
        return false;
    }

    public bool SetVfxConfig(string name, float startBeat, float endBeat) {
        if(!VfxData.TryGetValue(name, out var vfxData)) {
            Plugin.Log.LogWarning($"VFX config '{name}' not found.");
            return false;
        }

        var oldVfx = Transition.Transition?.Transition.EndState; // lol
        if(!oldVfx) {
            oldVfx = Instance._riftFXConfig.CharacterRiftColorConfig;
        }
        if(!oldVfx) {
            oldVfx = Instance._rhythmRiftBackgroundFx.DefaultRiftFXColorConfig;
        }
        Transition.StartTransition(new VfxTransition(oldVfx!, vfxData, startBeat, endBeat), UpdateVfx);

        return true;
    }

    public void UpdateVfx(RiftFXColorConfig vfx) {
        if(Transition == null) {
            return;
        }

        var fx = Instance._riftFXConfig;
        fx.CharacterRiftColorConfig = vfx;
        
        var background = Instance._rhythmRiftBackgroundFx;
        if(background) {
            var coreParticles = background._coreParticleSystem;
            if(coreParticles) {
                var main = coreParticles.main;
                main.startColor = new(vfx.CoreStartColor1, vfx.CoreStartColor2);

                var colorOverLifetime = coreParticles.colorOverLifetime;
                colorOverLifetime.color = vfx.CoreColorOverLifetime;
            }

            var speedlines = background._speedLines;
            if(speedlines) {
                var main = speedlines.main;
                main.startColor = vfx.SpeedlinesStartColor;

                var colorOverLifetime = speedlines.colorOverLifetime;
                colorOverLifetime.color = vfx.SpeedlinesColorOverLifetime;
            }

            var backgroundMaterial = vfx.BackgroundMaterial;
            background._currentBGMaterial = backgroundMaterial;
            background._background.material = backgroundMaterial;
            background._riftGlowColor = vfx.RiftGlowColor;

            var characterParticles = background._customCharacterParticles;
            if(characterParticles) {
                var main = characterParticles.main;
                main.startColor = new(vfx.CustomParticleColor1, vfx.CustomParticleColor2);
                
                var colorOverLifetime = characterParticles.colorOverLifetime;
                colorOverLifetime.color = vfx.CustomParticleColorOverLifetime;
                background._customParRend.material = vfx.CustomParticleMaterial;

                // TODO: implement rotation

                var textureSheetAnimation = characterParticles.textureSheetAnimation;
                textureSheetAnimation.numTilesX = vfx.CustomParticleSheetSize?.x ?? 2;
                textureSheetAnimation.numTilesY = vfx.CustomParticleSheetSize?.y ?? 2;
            }

            var particleVfx = background._particleVFX;
            if(particleVfx != null) {
                foreach(var particleSystem in particleVfx) {
                    if(particleSystem) {
                        var main = particleSystem.main;
                        main.startColor = vfx.StrobeColor1; // this is how the game does it...
                    }
                }
            }

            var particleGradientVfx = background._particleGradientVFX;
            if(particleGradientVfx != null) {
                foreach(var particleSystem in particleGradientVfx) {
                    if(particleSystem) {
                        var main = particleSystem.main;
                        main.startColor = new(vfx.StrobeColor1, vfx.StrobeColor2);
                    }
                }
            }
            
            background.RefreshRiftMaterialProperties();
        }
    }
}

[HarmonyPatch(typeof(RRStageController))]
public static class StagePatch {
    [HarmonyPatch(nameof(RRStageController.UnpackScenePayload))]
    [HarmonyPostfix]
    public static void UnpackScenePayload(RRStageController __instance, ScenePayload currentScenePayload) {
        var portrait = PortraitState.Of(__instance._portraitUiController);
        portrait.LevelId = currentScenePayload.GetLevelId();
        if(currentScenePayload is RhythmRiftScenePayload rrPayload && rrPayload.TrackMetadata is LocalTrackMetadata metadata) {
            var state = StageState.Of(__instance);
            state.BasePath = metadata.BasePath ?? "";
            state.TryLoadVfxConfigs();
        }
    }

    [HarmonyPatch(nameof(RRStageController.CounterpartPortraitOverride), MethodType.Getter)]
    [HarmonyPostfix]
    public static void CounterpartPortraitOverride(RRStageController __instance, ref string? __result) {
        StageScenePayload stageScenePayload = __instance._stageScenePayload;
        if(stageScenePayload != null) {
            if(Config.ExtraModes.DisableBeastmaster && __result == __instance.BeastmasterPortraitCharacterId && !__instance._isCalibrationTest) {
                __result = null;
            }

            if(Config.ExtraModes.DisableShopkeeper && __result == __instance.ShopkeeperPortraitCharacterId) {
                __result = null;
            }

            if(Config.ExtraModes.DisableCoda && __result == __instance.CodaPortraitCharacterId) {
                __result = null;
            }
        }
    }

    [HarmonyPatch(nameof(RRStageController.StageInitialize))]
    [HarmonyPostfix]
    public static void StageInitialize(RRStageController __instance, ref IEnumerator __result) {
        // since the original function is a coroutine, we need to wrap the output to properly postfix
        var original = __result;
        __result = Wrapper();

        IEnumerator Wrapper() {
            CustomEvent.FlagAllForProcessing(__instance._beatmaps);

            yield return original;

            var state = StageState.Of(__instance);

            var ui = __instance._portraitUiController;
            var counterpart = PortraitViewState.Of(ui._counterpartPortraitViewInstance);
            var hero = PortraitViewState.Of(ui._heroPortraitViewInstance);

            var beatmapPlayer = BeatmapState.Of(__instance.BeatmapPlayer);
            beatmapPlayer.Stage = state;
            beatmapPlayer.Counterpart = counterpart;
            beatmapPlayer.Hero = hero;

            var tasks = new List<IEnumerator>();

            foreach(var setPortraitEvent in CustomEvent.Enumerate<SetPortraitEvent>(__instance._beatmaps)) {
                var animator = setPortraitEvent.IsHero ? hero : counterpart;
                animator?.PreloadPortrait(state.BasePortraitPath, setPortraitEvent.Name)
                    .Pipe(AsyncUtils.WaitForTask)
                    .Pipe(tasks.Add);
            }

            foreach(var task in tasks) {
                yield return task;
            }
        }
    }

    [HarmonyPatch(nameof(RRStageController.Update))]
    [HarmonyPostfix]
    public static void Update(RRStageController __instance) {
        if(__instance.BeatmapPlayer.IsPlaying()) {
            var state = StageState.Of(__instance);
            var beat = __instance.BeatmapPlayer.FmodTimeCapsule.TrueBeatNumber;
            state.Transition.Update(beat);
        }
    }
}
