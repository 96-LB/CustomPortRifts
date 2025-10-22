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

namespace CustomPortRifts.Patches;


public class PortraitData(Dictionary<string, DataDrivenAnimator.AnimationType> animations, ITrackPortrait metadata) {
    public Dictionary<string, DataDrivenAnimator.AnimationType> Animations { get; } = animations;
    public ITrackPortrait Metadata { get; } = metadata;
}

public class PortraitViewState : State<RRPortraitView, PortraitViewState> {
    public AnimatorState? Animator => Instance._dataDrivenAnimator?.Pipe(AnimatorState.Of);

    public TransitionManager<float> FadeTransition { get; } = new();
    public TransitionManager<PortraitData> PortraitTransition { get; } = new();
    public TransitionManager<Color> ColorTransition { get; } = new();

    public static Dictionary<string, PortraitData> Portraits { get; } = [];
    public Vector2 Offset { get; set; } = Vector2.zero;

    public async Task<bool> PreloadPortrait(string baseDir, string name) {
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
        if(Animator == null) {
            Plugin.Log.LogWarning($"Failed to set portrait color because no custom portrait animator exists.");
            return false;
        }

        Plugin.Log.LogMessage($"Setting portrait color to {color} at beat {startBeat} with a transition duration of {duration} beats.");
        var startColor = ColorTransition.IsTransitioning ? ColorTransition.EndState : Animator.Color;
        ColorTransition.StartTransition(new ColorTransition(startColor, color, startBeat, duration), Animator.UpdateColor);
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

    public void UpdateTransitions(float beat) {
        FadeTransition.Update(beat);
        PortraitTransition.Update(beat);
        ColorTransition.Update(beat);
    }
}

[HarmonyPatch(typeof(RRPortraitView))]
public static class PortraitViewPatch {
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