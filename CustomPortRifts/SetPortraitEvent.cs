using JetBrains.Annotations;
using Shared.RhythmEngine;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace CustomPortRifts;


public class SetPortraitEvent(string name, bool isHero) {
    public string Name { get; } = name;
    public bool IsHero { get; } = isHero;

    public static bool DoesTypeMatch(BeatmapEvent beatmapEvent, string type) {
        var types = type.ToLowerInvariant().Split('.');
        var beatmapTypes = beatmapEvent.type.ToLowerInvariant().Split(' ');
        for(int i = 0; i < types.Length; i++) {
            var partialType = string.Join('.', types[i..]);
            if(beatmapTypes.Contains(partialType)) {
                return true;
            }
        }
        return false;
    }
    
    public static bool TryParse(BeatmapEvent beatmapEvent, [MaybeNullWhen(false)] out SetPortraitEvent setPortraitEvent) {
        setPortraitEvent = null;
        if(!DoesTypeMatch(beatmapEvent, Constants.Events.SetPortrait)) {
            return false;
        }

        var name = beatmapEvent.GetFirstEventDataAsString(Constants.Keys.PortraitName);
        if(name == null) {
            return false;
        }
        var isHero = beatmapEvent.GetFirstEventDataAsBool(Constants.Keys.IsHero) ?? false;

        setPortraitEvent = new(name, isHero);
        return true;
    }
}
