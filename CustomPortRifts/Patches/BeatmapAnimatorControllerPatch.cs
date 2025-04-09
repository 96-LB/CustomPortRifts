using HarmonyLib;
using Shared.PlayerData;
using Shared.RhythmEngine;
using UnityEngine;
using UnityEngine.UI;

namespace CustomPortRifts.Patches;


using P = BeatmapAnimatorController;
using BeatmapState = State<BeatmapAnimatorController, BeatmapData>;
public class BeatmapData {
    public Portrait Portrait { get; set; }
    public Image Image { get; set; }

    public bool HasSprites => Portrait?.HasSprites ?? false;
}

[HarmonyPatch(typeof(P), nameof(P.UpdateSystem))]
public static class BeatmapAnimatorControllerPatch {
    public static void Postfix(
        P __instance,
        FmodTimeCapsule fmodTimeCapsule
    ) {
        var state = BeatmapState.Of(__instance);
        if(!state.HasSprites) {
            return;
        }
        
        if(PlayerSaveController.Instance.GetShouldShowStaticPortraits()) {
            // don't animate when static portraits are enabled
            state.Image.sprite = state.Portrait.NormalSprites[0];
            return;
        }
        
        Sprite[] sprites = state.Portrait.ActiveSprites;
        float beat = Mathf.Max(fmodTimeCapsule.TrueBeatNumber, 0) % 1;
        int frame = Mathf.FloorToInt(beat * 31); // portraits are animated at 31 fps, for some reason

        // frame 1 lasts 3 frames and the rest last 2
        int spriteIndex = Mathf.Max(1, Mathf.FloorToInt(frame + 1) / 2);
        if(spriteIndex >= sprites.Length) {
            spriteIndex = 0;
        }

        state.Image.sprite = sprites[spriteIndex];
        // TODO: set the offset here
    }
}
