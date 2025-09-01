using RhythmRift;
using Shared.TrackData;
using Shared.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CustomPortRifts.Patches;


public class PortraitData(Dictionary<string, DataDrivenAnimator.AnimationType> animations, ITrackPortrait metadata) {
    public Dictionary<string, DataDrivenAnimator.AnimationType> Animations { get; } = animations;
    public ITrackPortrait Metadata { get; } = metadata;
}

public class PortraitViewState : State<RRPortraitView, PortraitViewState> {
    public Dictionary<string, PortraitData> Portraits { get; } = [];

    public DataDrivenAnimator Animator => Instance._dataDrivenAnimator;
    public bool HasAnimator => Instance._dataDrivenAnimator != null;

    public async Task<bool> PreloadPortrait(string baseDir, string name) {
        if(Portraits.ContainsKey(name)) {
            Plugin.Log.LogInfo($"Portrait '{name}' is already preloaded.");
            return false;
        }
        
        if(!HasAnimator) {
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

    public bool SetPortrait(string name) {
        if(!HasAnimator) {
            Plugin.Log.LogWarning($"Failed to set portrait '{name}' because no data driven animator was found.");
            return false;
        }

        if(!Portraits.TryGetValue(name, out var portrait)) {
            Plugin.Log.LogWarning($"Portrait '{name}' not found.");
            return false;
        }

        Animator._animations = portrait.Animations;
        Instance._hasVibePowerAnimation = Animator.IsValidAnimation("VibePower");
        Instance._characterMaskImage.enabled = Instance._characterMask.enabled = !portrait.Metadata.DisableMask;
        Instance._characterTransform.anchoredPosition = new((float)portrait.Metadata.OffsetX, (float)portrait.Metadata.OffsetY);
        
        return true;
    }
    // TODO: portrait.json isn't being reset here
}

