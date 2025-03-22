using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;
using UnityEngine;
using UnityEngine.UI;

namespace CustomPortRifts.Patches;


using P = BeatmapAnimatorController;
using BeatmapState = State<BeatmapAnimatorController, BeatmapData>;
public class BeatmapData {
    public Portrait Portrait { get; set; }
}

[HarmonyPatch(typeof(P), nameof(P.UpdateSystem))]
public static class BeatmapAnimatorControllerPatch {
    public static void Postfix(
        P __instance,
        Animator ____animator,
        FmodTimeCapsule fmodTimeCapsule
    ) {
        var portrait = BeatmapState.Of(__instance).Portrait;
        if(portrait == null || !portrait.UsingCustomSprites || !____animator) {
            return;
        }

        ____animator.enabled = false;

        Sprite[] sprites = portrait.ActiveSprites;
        float beat = Mathf.Max(fmodTimeCapsule.TrueBeatNumber, 0) % 1;
        int frame = Mathf.FloorToInt(beat * 62);

        // frame 1 lasts 6 frames and the rest last 4
        int spriteIndex = Mathf.Max(1, Mathf.FloorToInt(frame + 2) / 4);
        if(spriteIndex >= sprites.Length) {
            spriteIndex = 0;
        }

        Image image = ____animator.transform.Find("MaskImage").Find("CharacterImage").GetComponent<Image>();
        image.sprite = sprites[spriteIndex];
    }
}
