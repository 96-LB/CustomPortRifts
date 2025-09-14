using CustomPortRifts.BeatmapEvents;
using HarmonyLib;
using Newtonsoft.Json;
using RhythmRift;
using Shared.PlayerData;
using Shared.SceneLoading.Payloads;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CustomPortRifts.Patches;


public class VfxData(LocalTrackVfxConfig config, Texture2D? particleTexture) {
    public LocalTrackVfxConfig Config { get; } = config;
    public Texture2D? ParticleTexture { get; } = particleTexture;
    public bool HasCustomParticles => ParticleTexture;
}

public class StageState : State<RRStageController, StageState> {
    public const string CUSTOMPORTRIFTS = "CustomPortRifts";
    public const string VFX_JSON = "vfx.json";

    public string BasePath { get; set; } = "";
    public string BasePortraitPath => Path.Combine(BasePath, CUSTOMPORTRIFTS);
    public string VfxPath => Path.Combine(BasePortraitPath, VFX_JSON);
    public Dictionary<string, VfxData> VfxData { get; } = [];

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
        var text = FileUtils.ReadCompressedString(VfxPath);
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

    public bool SetVfxConfig(string name) {
        if(!VfxData.TryGetValue(name, out var vfxData)) {
            Plugin.Log.LogWarning($"VFX config '{name}' not found.");
            return false;
        }

        var fx = Instance._riftFXConfig;
        var fxController = Instance._rhythmRiftBackgroundFx;
        var vfx = fx.CharacterRiftColorConfig;
        if(!vfx) {
            vfx = fxController.DefaultRiftFXColorConfig;
        }
        vfx = Object.Instantiate(vfx);

        var newVfx = vfxData.Config;
        vfx.CoreStartColor1 = newVfx.CoreStartColor1 ?? vfx.CoreStartColor1;
        vfx.CoreStartColor2 = newVfx.CoreStartColor2 ?? vfx.CoreStartColor2;
        vfx.SpeedlinesStartColor = newVfx.SpeedlinesStartColor ?? vfx.SpeedlinesStartColor;
        vfx.CoreColorOverLifetime = newVfx.CoreColorOverLifetime ?? vfx.CoreColorOverLifetime;
        vfx.SpeedlinesColorOverLifetime = newVfx.SpeedlinesColorOverLifetime ?? vfx.SpeedlinesColorOverLifetime;
        vfx.RiftGlowColor = newVfx.RiftGlowColor ?? vfx.RiftGlowColor;
        vfx.StrobeColor1 = newVfx.StrobeColor1 ?? vfx.StrobeColor1;
        vfx.StrobeColor2 = newVfx.StrobeColor2 ?? vfx.StrobeColor2;
        vfx.CustomParticleColor1 = newVfx.CustomParticleColor1 ?? vfx.CustomParticleColor1;
        vfx.CustomParticleColor2 = newVfx.CustomParticleColor2 ?? vfx.CustomParticleColor2;
        vfx.CustomParticleColorOverLifetime = newVfx.CustomParticleColorOverLifetime ?? vfx.CustomParticleColorOverLifetime;

        if(vfx.BackgroundMaterial) {
            vfx.BackgroundMaterial = new Material(vfx.BackgroundMaterial);
            newVfx.BackgroundColor1?.Pipe(x => vfx.BackgroundMaterial.SetColor("_TopColor", x));
            newVfx.BackgroundColor2?.Pipe(x => vfx.BackgroundMaterial.SetColor("_BottomColor", x));
            newVfx.BackgroundGradientIntensity?.Pipe(x => vfx.BackgroundMaterial.SetFloat("_GradientIntensity", x));
            newVfx.BackgroundAdditiveIntensity?.Pipe(x => vfx.BackgroundMaterial.SetFloat("_AdditiveIntensity", x));
        }

        if(vfxData.ParticleTexture) {
            vfx.CustomParticleMaterial = new Material(vfx.CustomParticleMaterial);
            vfx.CustomParticleMaterial.SetTexture("_Texture2D", vfxData.ParticleTexture);
            vfx.CustomParticleSheetSize = new(newVfx.CustomParticleSheetWidth ?? 2, newVfx.CustomParticleSheetHeight ?? 2);
        }

        fx.CharacterRiftColorConfig = vfx;

        var bgDetail = PlayerSaveController.Instance.GetBackgroundDetailLevel();
        fxController.SetConfig(fx, Instance.BeatmapPlayer, bgDetail == BackgroundDetailLevel.NoBackground);
        return true;
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
}
