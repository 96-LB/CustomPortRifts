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

public class VfxTransition(RiftFXColorConfig oldVfx, VfxData vfxData, float startBeat, float duration) {
    public RiftFXColorConfig NewVfx => InterpolateVfx(1);

    public float BeatToProgress(float beat) => duration <= 0 ? 1 : Mathf.Clamp01((beat - startBeat) / duration);
    public RiftFXColorConfig Evaluate(float beat) => InterpolateVfx(BeatToProgress(beat));
    public RiftFXColorConfig InterpolateVfx(float t) {
        Plugin.Log.LogMessage(t);

        // TODO: remove code dupe
        var vfx = Object.Instantiate(oldVfx);
        vfx.CoreStartColor1 = oldVfx.CoreStartColor1;
        vfx.CoreStartColor2 = oldVfx.CoreStartColor2;
        vfx.SpeedlinesStartColor = oldVfx.SpeedlinesStartColor;
        vfx.CoreColorOverLifetime = oldVfx.CoreColorOverLifetime;
        vfx.SpeedlinesColorOverLifetime = oldVfx.SpeedlinesColorOverLifetime;
        vfx.RiftGlowColor = oldVfx.RiftGlowColor;
        vfx.StrobeColor1 = oldVfx.StrobeColor1;
        vfx.StrobeColor2 = oldVfx.StrobeColor2;
        vfx.CustomParticleColor1 = oldVfx.CustomParticleColor1;
        vfx.CustomParticleColor2 = oldVfx.CustomParticleColor2;
        vfx.CustomParticleColorOverLifetime = oldVfx.CustomParticleColorOverLifetime;
        vfx.BackgroundMaterial = oldVfx.BackgroundMaterial;
        vfx.CustomParticleMaterial = oldVfx.CustomParticleMaterial;
        vfx.CustomParticleSheetSize = oldVfx.CustomParticleSheetSize;

        var newVfx = vfxData.Config;
        newVfx.CoreStartColor1?.Pipe(x => vfx.CoreStartColor1 = GradientUtil.Lerp(vfx.CoreStartColor1, x, t));
        newVfx.CoreStartColor2?.Pipe(x => vfx.CoreStartColor2 = GradientUtil.Lerp(vfx.CoreStartColor2, x, t));
        newVfx.SpeedlinesStartColor?.Pipe(x => vfx.SpeedlinesStartColor = GradientUtil.Lerp(vfx.SpeedlinesStartColor, x, t));
        newVfx.CoreColorOverLifetime?.Pipe(x => vfx.CoreColorOverLifetime = GradientUtil.Lerp(vfx.CoreColorOverLifetime, x, t));
        newVfx.SpeedlinesColorOverLifetime?.Pipe(x => vfx.SpeedlinesColorOverLifetime = GradientUtil.Lerp(vfx.SpeedlinesColorOverLifetime, x, t));
        newVfx.RiftGlowColor?.Pipe(x => vfx.RiftGlowColor = Color.Lerp(vfx.RiftGlowColor, x, t));
        newVfx.StrobeColor1?.Pipe(x => vfx.StrobeColor1 = Color.Lerp(vfx.StrobeColor1, x, t));
        newVfx.StrobeColor2?.Pipe(x => vfx.StrobeColor2 = Color.Lerp(vfx.StrobeColor2, x, t));
        newVfx.CustomParticleColor1?.Pipe(x => vfx.CustomParticleColor1 = GradientUtil.Lerp(vfx.CustomParticleColor1, x, t));
        newVfx.CustomParticleColor2?.Pipe(x => vfx.CustomParticleColor2 = GradientUtil.Lerp(vfx.CustomParticleColor2, x, t));
        newVfx.CustomParticleColorOverLifetime?.Pipe(x => vfx.CustomParticleColorOverLifetime = GradientUtil.Lerp(vfx.CustomParticleColorOverLifetime, x, t));

        var mat = vfx.BackgroundMaterial;
        if(mat) {
            var newMat = new Material(mat);
            newVfx.BackgroundColor1?.Pipe(x => newMat.SetColor("_TopColor", Color.Lerp(mat.GetColor("_TopColor"), x, t)));
            newVfx.BackgroundColor2?.Pipe(x => newMat.SetColor("_BottomColor", Color.Lerp(mat.GetColor("_BottomColor"), x, t)));
            newVfx.BackgroundGradientIntensity?.Pipe(x => newMat.SetFloat("_GradientIntensity", Mathf.Lerp(mat.GetFloat("_GradientIntensity"), x, t)));
            newVfx.BackgroundAdditiveIntensity?.Pipe(x => newMat.SetFloat("_AdditiveIntensity", Mathf.Lerp(mat.GetFloat("_AdditiveIntensity"), x, t)));
            vfx.BackgroundMaterial = newMat;
        }

        if(vfxData.ParticleTexture && t >= 0.5f) {
            vfx.CustomParticleMaterial = new Material(vfx.CustomParticleMaterial);
            vfx.CustomParticleMaterial.SetTexture("_Texture2D", vfxData.ParticleTexture);
            vfx.CustomParticleSheetSize = new(newVfx.CustomParticleSheetWidth ?? 2, newVfx.CustomParticleSheetHeight ?? 2);
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

    public bool SetVfxConfig(string name, float startBeat, float endBeat) {
        if(!VfxData.TryGetValue(name, out var vfxData)) {
            Plugin.Log.LogWarning($"VFX config '{name}' not found.");
            return false;
        }

        var oldVfx = Transition?.NewVfx;
        if(!oldVfx) {
            oldVfx = Instance._riftFXConfig.CharacterRiftColorConfig;
        }
        if(!oldVfx) {
            oldVfx = Instance._rhythmRiftBackgroundFx.DefaultRiftFXColorConfig;
        }
        Plugin.Log.LogError($"{startBeat} {endBeat}");
        Transition = new(oldVfx!, vfxData, startBeat, endBeat);

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
