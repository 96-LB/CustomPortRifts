using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using RhythmRift;
using Shared.RhythmEngine;
using Shared.SceneLoading.Payloads;
using UnityEngine;
using UnityEngine.UI;

namespace CustomPortRifts.Patches;


[HarmonyPatch(typeof(RRPortraitView), "Awake")]
internal static class TestPatch {
    public static void Postfix(
        RRPortraitView __instance
    ) {
        DebugUtil.PrintAllChildren(__instance, true, true);
    }
}


[HarmonyPatch(typeof(BeatmapAnimatorController), nameof(BeatmapAnimatorController.UpdateSystem))]
public static class TestPatch2 {
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
            // TODO: we probably want some animator functions
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

        // List<object> values = new();
        // RectTransform transform = ____animator.transform.Find("MaskImage").Find("CharacterImage").GetComponent<RectTransform>();
        // Image image = transform.GetComponent<Image>();


        // values.Add(____animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1);
        // values.Add(image.sprite.name[^1]);

        // Plugin.Log.LogMessage(string.Join(", ", values));

        //DebugUtil.PrintAllChildren(__instance, true, true);
    }
}


[HarmonyPatch(typeof(RRStageController), "UnpackScenePayload")]
internal static class TestPatch3 {
    public static void Postfix(
        ScenePayload currentScenePayload
    ) {
        TestPatch2.performanceLevel = RRPerformanceLevel.Normal;
        TestPatch2.normalSprites = null;
        TestPatch2.wellSprites = null;
        TestPatch2.poorlySprites = null;
        TestPatch2.vibePowerSprites = null;

        if(currentScenePayload is not RRCustomTrackScenePayload payload) {
            return;
        }

        var dir = Path.GetDirectoryName(payload.GetBeatmapFileName());
        dir = Path.Combine(dir, "CustomPortRifts");
        if(!Directory.Exists(dir)) {
            Plugin.Log.LogInfo("No custom portrait folder found. Folder should be called 'CustomPortRifts' and be located in the same directory as the beatmap. Falling back to default.");
            return;
        }

        Sprite[] LoadSprites(string dirName) {
            var fullDir = Path.Combine(dir, dirName);
            List<Sprite> sprites = [];
            if(Directory.Exists(fullDir)) {
                var files = Directory.GetFiles(fullDir, "*.png");
                Array.Sort(files);
                foreach(var file in files) {
                    var bytes = File.ReadAllBytes(file);
                    var texture = new Texture2D(1, 1);
                    try {
                        // TODO: this is slow! consider handling this asynchronously to avoid game freeze
                        // TODO: loading this on every reload is really bad
                        texture.LoadImage(bytes);
                        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        sprites.Add(sprite);
                    } catch(Exception e) {
                        Plugin.Log.LogError($"Failed to load image {file}: {e}");
                    }
                }
            }
            return sprites.Count > 0 ? [..sprites] : null;
        }

        var normalSprites = LoadSprites("Normal");
        var poorlySprites = LoadSprites("DoingPoorly");
        var wellSprites = LoadSprites("DoingWell");
        var vibePowerSprites = LoadSprites("VibePower");

        normalSprites ??= wellSprites ?? poorlySprites ?? vibePowerSprites;
        wellSprites ??= normalSprites;
        poorlySprites ??= normalSprites;
        vibePowerSprites ??= wellSprites;

        if(normalSprites == null) {
            Plugin.Log.LogInfo("No custom portrait sprites found, though the folder exists. Sprites should be .png format and located in subfolders called 'Normal', 'DoingPoorly', or 'DoingWell'. Falling back to default.");
            return;
        }

        TestPatch2.normalSprites = normalSprites;
        TestPatch2.wellSprites = wellSprites;
        TestPatch2.poorlySprites = poorlySprites;
        TestPatch2.vibePowerSprites = vibePowerSprites;
    }
}

[HarmonyPatch(typeof(RRPortraitUiController), nameof(RRPortraitUiController.UpdateDisplay))]
internal static class TestPatch4 {
    public static void Postfix(
        RRPerformanceLevel performanceLevel
    ) {
        TestPatch2.performanceLevel = performanceLevel;
    }
}

[HarmonyPatch(typeof(RRPortraitUiController), "LoadCharacterPortrait")]
internal static class TestPatch5 {
    public static void Postfix(
        RRPortraitUiController __instance,
        bool isHeroPortrait
    ) {
        if(!TestPatch2.UsingCustomSprites || isHeroPortrait) {
            return;
        }

        // TODO: error handling
        // TODO: add something for onconfigchange
        var portrait = __instance.Field<RRPortraitView>("_counterpartPortraitViewInstance").Value;
        DebugUtil.PrintAllChildren(portrait, true, true);
        var image = portrait.transform.Find("MaskImage").Find("CharacterImage").GetComponent<Image>();
        image.sprite = TestPatch2.normalSprites[0];
    }
}

// TODO: seems to be some issue with animator speed not getting set properly?
