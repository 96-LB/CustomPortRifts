using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;
using UnityEngine;
using UnityEngine.UI;

namespace CustomPortRifts.Patches;


using P = BeatmapAnimatorController;

[HarmonyPatch(typeof(P), nameof(P.UpdateSystem))]
public static class BeatmapAnimatorControllerPatch {
    public static RRPerformanceLevel performanceLevel;
    public static Sprite[] normalSprites; // TODO: these being public is dangerous
    public static Sprite[] wellSprites;
    public static Sprite[] vibePowerSprites;
    public static Sprite[] poorlySprites;
    public static bool UsingCustomSprites => Config.CustomPortraits.Enabled.Value && normalSprites != null && normalSprites.Length > 0;

    public static void Postfix(
        Animator ____animator,
        FmodTimeCapsule fmodTimeCapsule
    ) {
        // TODO: this check is jank; we can identify this in a better way
        if(!____animator.gameObject.name.StartsWith("RR") || ____animator.gameObject.name.Contains("Cadence")) {
            return;
        }

        if(____animator) {
            ____animator.enabled = !UsingCustomSprites;
        }

        if(!UsingCustomSprites) {
            return;
        }

        Sprite[] sprites = performanceLevel switch {
            RRPerformanceLevel.Awesome or RRPerformanceLevel.Amazing => wellSprites,
            RRPerformanceLevel.Poor or RRPerformanceLevel.Terrible or RRPerformanceLevel.GameOver => poorlySprites,
            RRPerformanceLevel.VibePower => vibePowerSprites,
            _ => normalSprites
        };

        float beat = Mathf.Max(fmodTimeCapsule.TrueBeatNumber, 0) % 1;
        int frame = Mathf.FloorToInt(beat * 62);

        // frame 1 lasts 6 frames and the rest last 4
        int spriteIndex = Mathf.Max(1, Mathf.FloorToInt(frame + 2) / 4);
        if(spriteIndex >= sprites.Length) {
            spriteIndex = 0;
        }

        // TODO: error handling
        Image image = ____animator.transform.Find("MaskImage").Find("CharacterImage").GetComponent<Image>();
        image.sprite = sprites[spriteIndex];
    }
}
