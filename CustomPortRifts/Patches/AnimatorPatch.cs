using HarmonyLib;
using Shared.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static Shared.Utilities.DataDrivenAnimator;

namespace CustomPortRifts.Patches;


public class AnimatorState : State<DataDrivenAnimator, AnimatorState> {
    public bool HasVibe => Instance.IsValidAnimation("VibePower");
    public Image? Image => Instance._targetImage;
    public Vector2 Position {
        get => Instance._targetImage != null ? Instance._targetImage.rectTransform.anchoredPosition : Vector2.zero;
        set {
            if(Instance._targetImage != null) {
                Instance._targetImage.rectTransform.anchoredPosition = value;
            }
        }
    }

    public Vector2 Offset { get; private set; } = Vector2.zero;
    public Vector2 OriginalSize { get; private set; } = Vector2.zero;

    public async Task<Dictionary<string, AnimationType>?> PreloadPortrait(Options options) {
        var temp = Instance._animations;
        Instance.Configure(options);
        var animations = Instance._animations;
        Instance._animations = temp;

        foreach(var animation in animations.Values) {
            foreach(var frame in animation.Frames) {
                await frame.SpriteTask; // preload all sprites
            }
        }

        return animations;
    }
    
    public void UpdateFade(float fade) {
        if(Image != null) {
            Image.color = new(1, 1, 1, 1 - fade);
        }
    }

    public void UpdateOffset(Vector2 offset, bool update = true) {
        Plugin.Log.LogMessage($"Updating offset to {offset}, update={update}");
        if(update && Image != null) {
            Image.rectTransform.anchoredPosition += offset - Offset;
        }
        Offset = offset;
    }

    public void UpdatePortrait(Dictionary<string, AnimationType> animations) {
        Instance._animations = animations;
        Instance.Refresh();
    }

    public void RefreshOffset() {
        if(
            Image == null
            || Instance.ActiveAnimationId == null
            || !Instance._animations.TryGetValue(Instance.ActiveAnimationId, out var frames)
            || frames.Frames.Count <= 0
        ) {
            return;
        }

        var num = 0.0;
        AnimationFrame? animationFrame = null;
        foreach(var frame in frames.Frames) {
            if(frame.Sprite != null) {
                animationFrame = frame;
            }

            num += frame.Duration;
            if(num > Instance.TargetTime) {
                break;
            }
        }

        var sprite = animationFrame?.Sprite;
        if(sprite != null) {
            if(OriginalSize == Vector2.zero) {
                OriginalSize = Image.rectTransform.sizeDelta;
            } else {
                Image.rectTransform.sizeDelta = OriginalSize;
            }

            if(animationFrame != null) {
                if(animationFrame.Offset.HasValue) {
                    var offset = animationFrame.Offset.Value - Position + Offset;
                    UpdateOffset(offset, true);
                }
            }
        }

    }
}

[HarmonyPatch(typeof(DataDrivenAnimator))]
public static class AnimatorPatch {
    [HarmonyPatch(nameof(DataDrivenAnimator.Refresh))]
    [HarmonyPrefix]
    public static void Refresh(DataDrivenAnimator __instance) {
        var state = AnimatorState.Of(__instance);
        state.RefreshOffset();
    }
}
