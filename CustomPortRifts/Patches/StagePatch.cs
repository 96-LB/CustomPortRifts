using CustomPortRifts.BeatmapEvents;
using HarmonyLib;
using Newtonsoft.Json;
using RhythmRift;
using Shared;
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

public class VfxTransition(RiftFXColorConfig oldVfx, VfxData vfxData, float startBeat, float duration, float particleFadeTime)
     : FadeTransition<RiftFXColorConfig>(startBeat, duration, particleFadeTime) {
    public override RiftFXColorConfig Interpolate(float t) {
        Plugin.Log.LogMessage(t);

        var vfx = Object.Instantiate(oldVfx);
        var newVfx = vfxData.Config;
        vfx.CoreStartColor1 = newVfx.CoreStartColor1.Lerp(oldVfx.CoreStartColor1, t);
        vfx.CoreStartColor2 = newVfx.CoreStartColor2.Lerp(oldVfx.CoreStartColor2, t);
        vfx.SpeedlinesStartColor = newVfx.SpeedlinesStartColor.Lerp(oldVfx.SpeedlinesStartColor, t);
        vfx.CoreColorOverLifetime = newVfx.CoreColorOverLifetime.Lerp(oldVfx.CoreColorOverLifetime, t);
        vfx.SpeedlinesColorOverLifetime = newVfx.SpeedlinesColorOverLifetime.Lerp(oldVfx.SpeedlinesColorOverLifetime, t);
        vfx.RiftGlowColor = Color.Lerp(newVfx.RiftGlowColor ?? oldVfx.RiftGlowColor, oldVfx.RiftGlowColor, t);
        vfx.StrobeColor1 = Color.Lerp(newVfx.StrobeColor1 ?? oldVfx.StrobeColor1, oldVfx.StrobeColor1, t);
        vfx.StrobeColor2 = Color.Lerp(newVfx.StrobeColor2 ?? oldVfx.StrobeColor2, oldVfx.StrobeColor2, t);
        vfx.CustomParticleColor1 = newVfx.CustomParticleColor1.Lerp(oldVfx.CustomParticleColor1, t);
        vfx.CustomParticleColor2 = newVfx.CustomParticleColor2.Lerp(oldVfx.CustomParticleColor2, t);
        vfx.CustomParticleColorOverLifetime = newVfx.CustomParticleColorOverLifetime.Lerp(oldVfx.CustomParticleColorOverLifetime, t);
        vfx.BackgroundMaterial = oldVfx.BackgroundMaterial;
        vfx.CustomParticleMaterial = oldVfx.CustomParticleMaterial;
        vfx.CustomParticleSheetSize = oldVfx.CustomParticleSheetSize;

        var oldMat = vfx.BackgroundMaterial;
        if(oldMat) {
            var newMat = new Material(oldMat);
            newVfx.BackgroundColor1?.Pipe(x => newMat.SetColor("_TopColor", Color.Lerp(oldMat.GetColor("_TopColor"), x, t)));
            newVfx.BackgroundColor2?.Pipe(x => newMat.SetColor("_BottomColor", Color.Lerp(oldMat.GetColor("_BottomColor"), x, t)));
            newVfx.BackgroundGradientIntensity?.Pipe(x => newMat.SetFloat("_GradientIntensity", Mathf.Lerp(oldMat.GetFloat("_GradientIntensity"), x, t)));
            newVfx.BackgroundAdditiveIntensity?.Pipe(x => newMat.SetFloat("_AdditiveIntensity", Mathf.Lerp(oldMat.GetFloat("_AdditiveIntensity"), x, t)));
            vfx.BackgroundMaterial = newMat;
        }

        if(vfxData.ParticleTexture) {
            if(t >= 0.5f) {
                vfx.CustomParticleMaterial = new Material(vfx.CustomParticleMaterial);
                vfx.CustomParticleMaterial.SetTexture("_Texture2D", vfxData.ParticleTexture);
                vfx.CustomParticleSheetSize = new(newVfx.CustomParticleSheetWidth ?? 2, newVfx.CustomParticleSheetHeight ?? 2);
            }

            vfx.CustomParticleColor1 = vfx.CustomParticleColor1.Lerp(Color.clear, FadeAmount(t));
            vfx.CustomParticleColor2 = vfx.CustomParticleColor2.Lerp(Color.clear, FadeAmount(t));
        }

        return vfx;
    }
}

public class StageState : State<RRStageController, StageState> {
    public const string CUSTOMPORTRIFTS = "CustomPortRifts";
    public const string VFX_JSON = "vfx.json";

    public string BasePath { get; set; } = "";
    public string BasePortraitPath => Path.Combine(BasePath, CUSTOMPORTRIFTS);
    public string VfxPath => Path.Combine(BasePortraitPath, VFX_JSON);
    public Dictionary<string, VfxData> VfxData { get; } = [];

    public VfxTransition? Transition { get; set; }

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
            Plugin.Log.LogError($"Failed to parse custom {VFX_JSON} file: {e.Message}");
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

    public bool SetVfxConfig(string name, float startBeat, float endBeat, float particleFadeTime) {
        if(!VfxData.TryGetValue(name, out var vfxData)) {
            Plugin.Log.LogWarning($"VFX config '{name}' not found.");
            return false;
        }

        var oldVfx = Transition?.EndState;
        if(!oldVfx) {
            oldVfx = Instance._riftFXConfig.CharacterRiftColorConfig;
        }
        if(!oldVfx) {
            oldVfx = Instance._rhythmRiftBackgroundFx.DefaultRiftFXColorConfig;
        }
        Plugin.Log.LogError($"{startBeat} {endBeat}");
        Transition = new(oldVfx!, vfxData, startBeat, endBeat, particleFadeTime);

        return true;
    }

    public void UpdateVfx(float beat) {
        if(Transition == null) {
            return;
        }

        var fx = Instance._riftFXConfig;
        Plugin.Log.LogError(beat);
        fx.CharacterRiftColorConfig = Transition.Evaluate(beat);

        var bgDetail = PlayerSaveController.Instance.GetBackgroundDetailLevel();
        Instance._rhythmRiftBackgroundFx.SetConfig(fx, Instance.BeatmapPlayer, bgDetail == BackgroundDetailLevel.NoBackground);

        if(Transition.BeatToProgress(beat) >= 1) {
            Transition = null;
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
            state.UpdateVfx(beat);
        }
    }
}
