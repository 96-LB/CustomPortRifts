using CustomPortRifts.Transitions;
using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace CustomPortRifts.Patches;


public class PortraitData(Dictionary<string, DataDrivenAnimator.AnimationType> animations, ITrackPortrait metadata) {
    public Dictionary<string, DataDrivenAnimator.AnimationType> Animations { get; } = animations;
    public ITrackPortrait Metadata { get; } = metadata;
}

public class PortraitViewState : State<RRPortraitView, PortraitViewState> {
    public static bool IsPortraitSwitchingEnabled { get; private set; }
    public static Dictionary<string, PortraitData> Portraits { get; } = [];

    public Image? BackupImage { get; private set; }
    public Color BackupColor { get; private set; } = Color.white;


    public TransitionManager<float> FadeTransition { get; } = new();
    public TransitionManager<PortraitData> PortraitTransition { get; } = new();
    public TransitionManager<Color> ColorTransition { get; } = new();
    
    public AnimatorState? Animator => Instance._dataDrivenAnimator?.Pipe(AnimatorState.Of);

    public static bool UpdatePortraitSwitching() => IsPortraitSwitchingEnabled = Config.General.PortraitSwitching;

    public void SetBackupImage() {
        if(BackupImage == null) {
            BackupImage = Instance.transform.Find("MaskImage")?.Find("CharacterImage")?.GetComponent<Image>();
        }
    }

    public async Task<bool> PreloadPortrait(string baseDir, string name) {
        if(!IsPortraitSwitchingEnabled) {
            Plugin.Log.LogInfo($"Skipping preload of portrait '{name}' because portrait switching is disabled.");
            return false;
        }

        if(Animator == null) {
            Plugin.Log.LogWarning($"Failed to preload portrait '{name}' because no custom portrait animator exists.");
            return false;
        }

        if(Portraits.ContainsKey(name)) {
            Plugin.Log.LogInfo($"Portrait '{name}' is already being preloaded.");
            return false;
        }

        var portrait = LocalTrackPortrait.TryLoadCustomPortrait(Path.Combine(baseDir, name), "CustomCounterpart");
        if(portrait == null) {
            Plugin.Log.LogWarning($"Failed to load portrait '{name}' from '{baseDir}'.");
            return false;
        }

        var options = new DataDrivenAnimator.Options {
            BasePath = portrait.CustomBasePath,
            Config = portrait.CustomAnimations ?? new(),
            LoadOrder = ["PerformingNormal", "PerformingWell", "PerformingPoorly", "VibePower"]
        };

        Portraits[name] = new([], portrait); // reserve spot
        Plugin.Log.LogMessage($"Preloading portrait '{name}'...");
        var animations = await Animator.PreloadPortrait(options);
        if(animations == null) {
            Plugin.Log.LogWarning($"Failed to preload animations for portrait '{name}'.");
            return false;
        }

        Portraits[name] = new(animations, portrait);

        Plugin.Log.LogMessage($"Finished preloading portrait '{name}'.");
        return true;
    }

    public bool SetPortrait(string name, float startBeat, float duration) {
        if(!IsPortraitSwitchingEnabled) {
            Plugin.Log.LogInfo($"Skipping portrait change to '{name}' because portrait switching is disabled.");
            return false;
        }

        if(Animator == null) {
            Plugin.Log.LogWarning($"Failed to set portrait '{name}' because no custom portrait animator exists.");
            return false;
        }

        if(!Portraits.TryGetValue(name, out var portrait)) {
            Plugin.Log.LogWarning($"Portrait '{name}' not found.");
            return false;
        }

        Plugin.Log.LogMessage($"Setting portrait to '{name}' at beat {startBeat} with a transition duration of {duration} beats.");
        FadeTransition.StartTransition(new FadeTransition(startBeat, duration), Animator.UpdateFade);
        PortraitTransition.StartTransition(new StaticTransition<PortraitData>(startBeat + duration / 2, portrait), UpdatePortrait);

        return true;
    }
    
    public bool SetPortraitColor(Color color, float startBeat, float duration) {
        if(!IsPortraitSwitchingEnabled) {
            Plugin.Log.LogInfo($"Skipping portrait color change to {color} because portrait switching is disabled.");
            return false;
        }

        if(Animator == null && !BackupImage) {
            Plugin.Log.LogWarning($"Failed to set portrait color to {color} because no custom portrait animator exists and the portrait image could not be found.");
            return false;
        }

        Plugin.Log.LogMessage($"Setting portrait color to {color} at beat {startBeat} with a transition duration of {duration} beats.");
        var startColor = ColorTransition.IsTransitioning ? ColorTransition.EndState : (Animator?.Color ?? BackupColor);
        ColorTransition.StartTransition(new ColorTransition(startColor, color, startBeat, duration), UpdateColor);
        return true;
    }

    public void UpdatePortrait(PortraitData portrait) {
        if(Animator == null) {
            return;
        }
        Instance._hasVibePowerAnimation = Animator.HasVibe;
        Instance._characterMaskImage.enabled = Instance._characterMask.enabled = !portrait.Metadata.DisableMask;
        Animator.UpdateOffset(new((float)portrait.Metadata.OffsetX, (float)portrait.Metadata.OffsetY));
        Animator.UpdatePortrait(portrait.Animations);
    }
    public void UpdateColor(Color color) {
        if(Animator != null) {
            Animator.UpdateColor(color);
        } else {
            BackupColor = color;
        }
    }

    public void UpdateTransitions(float beat) {
        FadeTransition.Update(beat);
        PortraitTransition.Update(beat);
        ColorTransition.Update(beat);
        if(Animator == null && BackupImage != null) {
            BackupImage.color = BackupColor;
        }
    }
}


[HarmonyPatch(typeof(RRPortraitView))]
public static class PortraitViewPatch {
    [HarmonyPatch(nameof(RRPortraitView.Start))]
    [HarmonyPostfix]
    public static void Start(RRPortraitView __instance) {
        var state = PortraitViewState.Of(__instance);
        state.SetBackupImage();
    }

    [HarmonyPatch(nameof(RRPortraitView.UpdateSystem))]
    [HarmonyPostfix]
    public static void UpdateSystem(RRPortraitView __instance, FmodTimeCapsule fmodTimeCapsule) {
        var state = PortraitViewState.Of(__instance);
        var beat = fmodTimeCapsule.TrueBeatNumber - 1;
        state.UpdateTransitions(beat);
    }

    [HarmonyPatch(nameof(RRPortraitView.ApplyCustomPortrait))]
    [HarmonyPostfix]
    public static void ApplyCustomPortrait(RRPortraitView __instance, ITrackPortrait portraitMetadata) {
        var animator = __instance._dataDrivenAnimator;
        if(animator == null) {
            return;
        }

        var state = AnimatorState.Of(animator);
        if(portraitMetadata != null) {
            // set update to false because the base method already applied the offset
            state.UpdateOffset(new((float)portraitMetadata.OffsetX, (float)portraitMetadata.OffsetY), update: false);
        }
    }
}
