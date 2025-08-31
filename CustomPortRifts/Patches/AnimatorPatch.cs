using HarmonyLib;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CustomPortRifts.Patches;


public class AnimatorState : State<DataDrivenAnimator, AnimatorState> {
    public Dictionary<string, Dictionary<string, DataDrivenAnimator.AnimationType>> Portraits { get; } = [];
    
    public async Task AddPortrait(string baseDir, string name) {
        if(Portraits.ContainsKey(name)) {
            return;
        }
        var portrait = LocalTrackPortrait.TryLoadCustomPortrait(Path.Combine(baseDir, name), "CustomCounterpart");
        if(portrait == null) {
            Plugin.Log.LogWarning($"Failed to load portrait '{name}' from '{baseDir}'.");
            return;
        }
        var options = new DataDrivenAnimator.Options {
            BasePath = portrait.CustomBasePath,
            Config = portrait.CustomAnimations ?? new(),
            LoadOrder = ["PerformingNormal", "PerformingWell", "PerformingPoorly", "VibePower"]
        };
        var temp = Instance._animations;
        Instance.Configure(options);
        Portraits[name] = Instance._animations;
        Instance._animations = temp;

        Plugin.Log.LogInfo($"Preloading portrait '{name}'...");
        foreach(var animation in Portraits[name].Values) {
            foreach(var frame in animation.Frames) {
                await frame.SpriteTask; // preload all sprites
            }
        }
        Plugin.Log.LogInfo($"Preloaded portrait '{name}'.");
    }

    public void SwitchPortrait(string name) {
        if(!Portraits.TryGetValue(name, out var animations)) {
            Plugin.Log.LogWarning($"Portrait '{name}' not found.");
            return;
        }
        Instance._animations = animations;
        // TODO: portrait.json isn't being reset here
    }
}

[HarmonyPatch(typeof(DataDrivenAnimator))]
public static class AnimatorPatch {

}
