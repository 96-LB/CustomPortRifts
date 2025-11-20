using BepInEx;
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
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomPortRifts.Patches;


public class StageState : State<RRStageController, StageState> {
    public const string CUSTOMPORTRIFTS = "CustomPortRifts";
    public const string VFX_JSON = "vfx.json";

    public static bool IsVfxSwitchingEnabled { get; private set; }

    public Dictionary<string, Texture2D> TextureCache { get; } = [];
    public Dictionary<string, VfxData> VfxData { get; } = [];
    public TransitionManager<RiftFXColorConfig> Transition { get; } = new();

    public string BasePath { get; set; } = "";
    public string BasePortraitPath => Path.Combine(BasePath, CUSTOMPORTRIFTS);
    public string VfxPath => Path.Combine(BasePortraitPath, VFX_JSON);

    public PortraitViewState Counterpart => PortraitViewState.Of(Instance._portraitUiController._counterpartPortraitViewInstance);
    public PortraitViewState Hero => PortraitViewState.Of(Instance._portraitUiController._heroPortraitViewInstance);
    public BeatmapState Beatmap => BeatmapState.Of(Instance.BeatmapPlayer);

    public float StartBeat => Mathf.Max(0,
        Instance._isPracticeMode
        ? Instance._practiceModeStartBeatNumber - Instance._practiceModeTotalBeatsSkippedBeforeStartBeatmap - Instance._microRiftMusicFadeInDurationInBeats
        : 0
    );

    public float EndBeat => Instance._isPracticeMode
        ? Instance._practiceModeEndBeatNumber
        : float.MaxValue;

    public bool ShouldUseCustomGraphics => Instance.CounterpartPortraitOverride == null;
    
    public static bool UpdateVfxSwitching(bool isEnabled) => IsVfxSwitchingEnabled = Config.General.VfxSwitching && isEnabled;

    public Texture2D? TryLoadParticleTexture(LocalTrackVfxConfig config) {
        var path = config.CustomParticleImagePath ?? "";
        if(path.IsNullOrWhiteSpace()) {
            return null;
        }
        if(TextureCache.TryGetValue(path, out var cachedTexture)) {
            return cachedTexture;
        }
        var bytes = FileUtils.ReadBytes(path);
        if(bytes == null) {
            return null;
        }

        var texture = new Texture2D(2, 2, TextureFormat.ARGB32, mipChain: false);
        if(!texture.LoadImage(bytes)) {
            return null;
        }

        TextureCache[path] = texture;
        return texture;
    }

    public bool TryLoadVfxConfigs() {
        if(!IsVfxSwitchingEnabled) {
            Plugin.Log.LogInfo("Skipping VFX loading because VFX switching is disabled.");
            return false;
        }

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

    public bool SetVfxConfig(string name, float startBeat, float duration) {
        if(!IsVfxSwitchingEnabled) {
            Plugin.Log.LogInfo($"Skipping VFX change to '{name}' because VFX switching is disabled.");
            return false;
        }

        if(!VfxData.TryGetValue(name, out var vfxData)) {
            Plugin.Log.LogWarning($"VFX config '{name}' not found.");
            return false;
        }

        var oldVfx = Transition.EndState;
        if(!oldVfx) {
            oldVfx = Instance._riftFXConfig.CharacterRiftColorConfig;
        }
        if(!oldVfx) {
            oldVfx = Instance._rhythmRiftBackgroundFx.DefaultRiftFXColorConfig;
        }

        Plugin.Log.LogMessage($"Setting VFX config to '{name}' at beat {startBeat} with a transition duration of {duration} beats.");
        Transition.StartTransition(new VfxTransition(oldVfx!, vfxData, startBeat, duration), UpdateVfx);

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

    public async Task Preload() {
        var beatmapPlayer = BeatmapState.Of(Instance.BeatmapPlayer);
        Beatmap.Stage = this;

        if(!ShouldUseCustomGraphics) {
            return;
        }
                
        // list all events which happen before the start beat
        var portraitEvents = CustomEvent.Enumerate<SetPortraitEvent>(Instance._beatmaps).ToList();
        var heroEvents = portraitEvents.Where(e => e.IsHero && e.Beat <= StartBeat).OrderByDescending(e => e.Beat).ToList();
        var counterpartEvents = portraitEvents.Where(e => !e.IsHero && e.Beat <= StartBeat).OrderByDescending(e => e.Beat).ToList();

        // we need to simulate the last event to set the correct portrait
        var lastHeroEvent = heroEvents.FirstOrDefault();
        var lastCounterpartEvent = counterpartEvents.FirstOrDefault();
        
        // if the last event is mid-fade, we also need to preload the event before it
        var lastHeroFadeEvent = lastHeroEvent?.PortraitChangeBeat > StartBeat ? heroEvents.Skip(1).FirstOrDefault() : null;
        var lastCounterpartFadeEvent = lastCounterpartEvent?.PortraitChangeBeat > StartBeat ? counterpartEvents.Skip(1).FirstOrDefault() : null;

        var portraitEventsToPreload = new[] { lastHeroEvent, lastCounterpartEvent, lastHeroFadeEvent, lastCounterpartFadeEvent };

        var tasks = new List<Task>();
        foreach(var setPortraitEvent in portraitEvents) {
            var beat = setPortraitEvent.Beat;
            if((StartBeat < beat && beat < EndBeat) || portraitEventsToPreload.Contains(setPortraitEvent)) {
                var animator = setPortraitEvent.IsHero ? Hero : Counterpart;
                animator.PreloadPortrait(BasePortraitPath, setPortraitEvent.Name).Pipe(tasks.Add);
            }
        }
        
        // color and vfx events are cheap enough to just process them all
        // in fact, we need to process all vfx events since they can combine nontrivially
        var colorEvents = CustomEvent.Enumerate<SetPortraitColorEvent>(Instance._beatmaps);
        var vfxEvents = CustomEvent.Enumerate<SetVfxEvent>(Instance._beatmaps);
        var eventsToProcess = colorEvents.Cast<CustomEvent>().Concat(vfxEvents);
        foreach(var customEvent in eventsToProcess) {
            if(StartBeat >= customEvent.Beat) {
                Beatmap.ProcessBeatEvent(customEvent);
            }
        }

        foreach(var task in tasks) {
            await task; // finish preloading all portraits
        }

        lastHeroFadeEvent?.Pipe(Beatmap.ProcessBeatEvent);
        lastCounterpartFadeEvent?.Pipe(Beatmap.ProcessBeatEvent);
        lastHeroEvent?.Pipe(Beatmap.ProcessBeatEvent);
        lastCounterpartEvent?.Pipe(Beatmap.ProcessBeatEvent);

        Hero.UpdateTransitions(StartBeat);
        Counterpart.UpdateTransitions(StartBeat);
        Transition.Update(StartBeat);
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
            StageState.UpdateVfxSwitching(__instance.CounterpartPortraitOverride == null);
            PortraitViewState.UpdatePortraitSwitching(__instance.CounterpartPortraitOverride == null); // TODO: better solution than static variable

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
            yield return AsyncUtils.WaitForTask(state.Preload());
        }
    }

    [HarmonyPatch(nameof(RRStageController.Update))]
    [HarmonyPostfix]
    public static void Update(RRStageController __instance) {
        if(__instance.BeatmapPlayer.IsPlaying()) {
            var state = StageState.Of(__instance);
            var beat = __instance.BeatmapPlayer.FmodTimeCapsule.TrueBeatNumber - 1;
            state.Transition.Update(beat);
        }
    }

    [HarmonyPatch(nameof(RRStageController.InitBackgroundVideo))]
    [HarmonyPostfix]
    public static void InitBackgroundVideo(RRStageController __instance, ref IEnumerator __result) {
        // since the original function is a coroutine, we need to wrap the output to properly postfix
        var original = __result;
        __result = Wrapper();

        IEnumerator Wrapper() {
            // we pretend practice mode is off when disable beastmaster is true so that the background video gets loaded
            var temp = __instance._isPracticeMode;
            __instance._isPracticeMode &= !Config.ExtraModes.DisableBeastmaster;

            yield return original;
            
            __instance._isPracticeMode = temp;
        }

    }
}
