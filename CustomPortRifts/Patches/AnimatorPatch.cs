using HarmonyLib;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections.Generic;
using System.IO;

namespace CustomPortRifts.Patches;


public class AnimatorState : State<DataDrivenAnimator, AnimatorState> {
    public Dictionary<string, Dictionary<string, DataDrivenAnimator.AnimationType>> Portraits { get; } = [];
    
    public void AddPortrait(string baseDir, string name) {
        Plugin.Log.LogMessage($"[{Instance.name}] AddPortrait: {name}");
        if(Portraits.ContainsKey(name)) {
            Plugin.Log.LogInfo("Portrait already loaded, skipping.");
            return;
        }
        var portrait = LocalTrackPortrait.TryLoadCustomPortrait(Path.Combine(baseDir, name), "CustomCounterpart");
        if(portrait == null) {
            Plugin.Log.LogWarning($"Failed to load custom portrait '{name}' from '{baseDir}'.");
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
        Plugin.Log.LogMessage($"[{Instance.name}] Loaded portrait '{name}' with {Portraits[name].Count} animations.");
        Plugin.Log.LogInfo(string.Join(' ', Portraits.Keys));
        Instance._animations = temp;
    }

    public void SwitchPortrait(string name) {
        Plugin.Log.LogMessage($"[{Instance.name}] SwitchPortrait: {name}");
        if(!Portraits.TryGetValue(name, out var animations)) {
            Plugin.Log.LogWarning($"Portrait '{name}' not found.");
            Plugin.Log.LogInfo(string.Join(' ', Portraits.Keys));
            return;
        }
        Instance._animations = animations;
    }
}

[HarmonyPatch(typeof(DataDrivenAnimator))]
public static class AnimatorPatch {

}
