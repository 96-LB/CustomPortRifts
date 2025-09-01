using Shared.RhythmEngine;
using System.Collections.Generic;
using System.Linq;

namespace CustomPortRifts.BeatmapEvents;


public abstract class CustomEvent {
    const string PREFIX = Plugin.NAME;
    const string GUID = Plugin.GUID;

    public BeatmapEvent BeatmapEvent { get; private set; } = default;
    public abstract string Type { get; }
    
    public string GetString(string key) {
        if(DoesTypeMatch()) {
            var value = BeatmapEvent.GetFirstEventDataAsString($"{GetMatchingType()}.{key}");
            if(!string.IsNullOrWhiteSpace(value)) {
                return value;
            }
        } 
        return BeatmapEvent.GetFirstEventDataAsString(key);
    }
    public bool? GetBool(string key) => BeatmapEvent.GetFirstEventDataAsBool(key);
    public int? GetInt(string key) => BeatmapEvent.GetFirstEventDataAsInt(key);
    public float? GetFloat(string key) => BeatmapEvent.GetFirstEventDataAsFloat(key);

    public string GetMatchingType() {
        var typeSegments = $"{PREFIX}.{Type}".ToLowerInvariant().Split('.');
        var typeMatches = new List<string>();
        for(int i = 0; i < typeSegments.Length; i++) {
            var partialType = string.Join('.', typeSegments[i..]);
            typeMatches.Add(partialType);
        }

        foreach(var type in BeatmapEvent.type.Split()) {
            if(typeMatches.Contains(type.ToLowerInvariant())) {
                return type;
            }
        }
        
        return "";
    }

    public bool DoesTypeMatch() {
        return !string.IsNullOrWhiteSpace(GetMatchingType());
    }

    public virtual bool IsValid() {
        if(!DoesTypeMatch()) {
            return false;
        }

        var matchingType = GetMatchingType();
        var types = BeatmapEvent.type.Split();
        foreach(var type in types) {
            var processor = GetString($"__MODS__.{type}");
            if(!string.IsNullOrEmpty(processor)) {
                return processor == GUID;
            } else if(type == matchingType) {
                return true; // no mod has claimed a higher-priority type
            }
        }
        return false; // this should never run
    }

    public void FlagForProcessing() {
        if(IsValid()) {
            BeatmapEvent.AddEventData($"__MODS__.{GetMatchingType()}", GUID);
        }
    }

    public static bool TryParse<T>(BeatmapEvent beatmapEvent, out T setPortraitEvent) where T : CustomEvent, new() {
        setPortraitEvent = new() {
            BeatmapEvent = beatmapEvent
        };
        return setPortraitEvent.IsValid();
    }

    public static IEnumerable<T> Enumerate<T>(IEnumerable<BeatmapEvent> events) where T : CustomEvent, new() {
        foreach(var beatmapEvent in events) {
            if(TryParse(beatmapEvent, out T customEvent)) {
                yield return customEvent;
            }
        }
    }

    public static IEnumerable<T> Enumerate<T>(Beatmap beatmap) where T : CustomEvent, new() {
        return Enumerate<T>(beatmap.BeatmapEvents);
    }

    public static IEnumerable<T> Enumerate<T>(IEnumerable<Beatmap> beatmaps) where T : CustomEvent, new() {
        return beatmaps.SelectMany(Enumerate<T>);
    }

    public static IEnumerable<CustomEvent> Enumerate(IEnumerable<BeatmapEvent> events) {
        var customEventTypes = typeof(CustomEvent).Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(typeof(CustomEvent)) && !type.IsAbstract && type.GetConstructor([]) != null)
            .ToArray();

        foreach(var beatmapEvent in events) {
            foreach(var customEventType in customEventTypes) {
                // dynamically call TryParse<T> for every subclass of CustomEvent
                var tryParseMethod = typeof(CustomEvent).GetMethod(nameof(TryParse)).MakeGenericMethod(customEventType);
                var parameters = new object[] { beatmapEvent, null! };
                var success = (bool)tryParseMethod.Invoke(null, parameters)!;
                if(success) {
                    yield return (CustomEvent)parameters[1];
                }
            }
        }
    }

    public static IEnumerable<CustomEvent> Enumerate(Beatmap beatmap) {
        return Enumerate(beatmap.BeatmapEvents);
    }

    public static IEnumerable<CustomEvent> Enumerate(IEnumerable<Beatmap> beatmaps) {
        return beatmaps.SelectMany(Enumerate);
    }

    public static void FlagAllForProcessing(IEnumerable<Beatmap> beatmaps) {
        foreach(var customEvent in Enumerate(beatmaps)) {
            customEvent.FlagForProcessing();
        }
    }
}
