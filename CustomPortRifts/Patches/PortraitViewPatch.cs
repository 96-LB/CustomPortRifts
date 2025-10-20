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
    public Dictionary<string, PortraitData> Portraits { get; } = [];

    public DataDrivenAnimator Animator => Instance._dataDrivenAnimator;

    public TransitionManager<float> FadeTransition { get; } = new();
    public TransitionManager<PortraitData> PortraitTransition { get; } = new();

    public Vector2 Offset { get; set; } = Vector2.zero;

    public async Task<bool> PreloadPortrait(string baseDir, string name) {
        if(Portraits.ContainsKey(name)) {
            Plugin.Log.LogInfo($"Portrait '{name}' is already preloaded.");
            return false;
        }
        
        if(!Animator) {
            Plugin.Log.LogWarning($"Failed to preload portrait '{name}' because no data driven animator was found.");
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
        var temp = Animator._animations;
        Animator.Configure(options);
        Portraits[name] = new(Animator._animations, portrait);
        Animator._animations = temp;

        foreach(var animation in Portraits[name].Animations.Values) {
            foreach(var frame in animation.Frames) {
                await frame.SpriteTask; // preload all sprites
            }
        }

        Plugin.Log.LogInfo($"Preloaded portrait '{name}'.");
        return true;
    }

    public bool SetPortrait(string name, float startBeat, float duration) {
        if(!Animator) {
            Plugin.Log.LogWarning($"Failed to set portrait '{name}' because no data driven animator was found.");
            return false;
        }

        if(!Portraits.TryGetValue(name, out var portrait)) {
            Plugin.Log.LogWarning($"Portrait '{name}' not found.");
            return false;
        }

        FadeTransition.StartTransition(new FadeTransition(startBeat, duration), UpdateFade);
        PortraitTransition.StartTransition(new StaticTransition<PortraitData>(startBeat + duration / 2, portrait), UpdatePortrait);

        return true;
    }

    public void UpdateFade(float fade) {
        if(Animator && Animator._targetImage != null) {
            Animator._targetImage.color = new (1, 1, 1, 1 - fade);
        }
    }
        
    public void UpdatePortrait(PortraitData portrait) {
        if(!Animator) {
            return;
        }
        
        Animator._animations = portrait.Animations;
        Instance._hasVibePowerAnimation = Animator.IsValidAnimation("VibePower");
        Instance._characterMaskImage.enabled = Instance._characterMask.enabled = !portrait.Metadata.DisableMask;
        UpdateOffset(portrait.Metadata.OffsetX, portrait.Metadata.OffsetY);
        Animator.Refresh();
    }

    public void UpdateOffset(double x, double y, bool update = true) {
        var offset = new Vector2((float)x, (float)y);
        if(update && Instance._characterTransform) {
            Instance._characterTransform.anchoredPosition += offset - Offset;
        }
        Offset = offset;
    }

    public void UpdateTransitions(float beat) {
        FadeTransition.Update(beat);
        PortraitTransition.Update(beat);
    }
}

[HarmonyPatch(typeof(RRPortraitView))]
public static class PortraitViewPatch {
    [HarmonyPatch(nameof(RRPortraitView.UpdateSystem))]
    [HarmonyPostfix]
    public static void UpdateSystem(RRPortraitView __instance, FmodTimeCapsule fmodTimeCapsule) {
        var state = PortraitViewState.Of(__instance);
        state.UpdateTransitions(fmodTimeCapsule.TrueBeatNumber);
    }

    [HarmonyPatch(nameof(RRPortraitView.ApplyCustomPortrait))]
    [HarmonyPostfix]
    public static void ApplyCustomPortrait(RRPortraitView __instance, ITrackPortrait portraitMetadata) {
        var state = PortraitViewState.Of(__instance);
        if(portraitMetadata != null) {
            // set update to false because the base method already applied the offset
            state.UpdateOffset(portraitMetadata.OffsetX, portraitMetadata.OffsetY, update: false);
        }
    }
}