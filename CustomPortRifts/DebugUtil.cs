using Shared;
using UnityEngine;

namespace CustomPortRifts;

public static class DebugUtil {
    private static void PrintAllComponents(GameObject gameObject, int depth = 1) {
        foreach(var component in gameObject.GetComponents<Component>()) {
            Plugin.Log.LogWarning($"{new string(' ', depth * 2)}• {component.GetType().Name}");
        }
    }

    internal static void PrintAllComponents(GameObject gameObject) {
        Plugin.Log.LogWarning($"Components of [{gameObject}]:");
        PrintAllComponents(gameObject, 1);
    }
    internal static void PrintAllComponents(Transform transform) {
        PrintAllComponents(transform.gameObject);
    }

    internal static void PrintAllComponents(Component component) {
        PrintAllComponents(component.gameObject);
    }

    private static void PrintAllChildren(Transform transform, int depth, bool recursive = false, bool components = false) {
        if(components) {
            PrintAllComponents(transform.gameObject, depth + 1);
        }
        foreach(Transform child in transform) {
            Plugin.Log.LogWarning($"{new string(' ', depth * 2)}○ {child.name}");
            if(recursive) {
                PrintAllChildren(child, depth + 1, recursive, components);
            }
        }
    }

    internal static void PrintAllChildren(Transform transform, bool recursive = false, bool components = false) {
        Plugin.Log.LogWarning($"Children of [{transform}]:");
        PrintAllChildren(transform, 1, recursive, components);
    }

    internal static void PrintAllChildren(GameObject gameObject, bool recursive = false, bool components = false) {
        PrintAllChildren(gameObject.transform, recursive, components);
    }

    internal static void PrintAllChildren(Component component, bool recursive = false, bool components = false) {
        PrintAllChildren(component.gameObject, recursive, components);
    }

    private static void Log(object message) {
        Plugin.Log.LogMessage(message);
    }

    private static void Header(object message) {
        Log($"*****{message}*****");
    }

    private static void Footer(object message) {
        Log(new string('*', message.ToString().Length + 10));
    }

    public static void Dump(AnimationClip clip) {
        Header(clip);
        Log($"wrap mode {clip.wrapMode}");
        Log($"{clip.frameRate} fps");
        Log($"{clip.length} seconds");
        Log($"{clip.events.Length} events:");
        foreach(var x in clip.events) {
            Log($"{x.time}: {x.functionName}({x.stringParameter}, {x.floatParameter}, {x.intParameter}, {x.objectReferenceParameter}) {x.messageOptions}");
        }
        Footer(clip);
    }

    public static void Dump(Sprite sprite) {
        Header(sprite);
        Log($"rect {sprite.rect}");
        Log($"pivot {sprite.pivot}");
        Log($"border {sprite.border}");
        Log($"pixels per unit {sprite.pixelsPerUnit}");
        Log($"packing mode {sprite.packingMode}");
        Footer(sprite);
    }
    
    public static void Dump(RiftFXConfig config) {
        Header(config);
        Dump(config.CharacterRiftColorConfig);
        Footer(config);
    }

    public static void Dump(RiftFXColorConfig config) {
        Header(config);
        Log($"bg mat {config.BackgroundMaterial}");
        Log("color1");
        Dump(config.CoreStartColor1);
        Log("color2");
        Dump(config.CoreStartColor2);
        Log("color");
        Dump(config.CoreColorOverLifetime);
        Log($"glow {config.RiftGlowColor}");
        Log($"strobe1 {config.StrobeColor1}");
        Log($"strobe2 {config.StrobeColor2}");
        Log("speedstart");
        Dump(config.SpeedlinesStartColor);
        Log("speed");
        Dump(config.SpeedlinesColorOverLifetime);
        Log("particle1");
        Dump(config.CustomParticleColor1);
        Log("particle2");
        Dump(config.CustomParticleColor2);
        Log("particle");
        Dump(config.CustomParticleColorOverLifetime);
        Log($"part mat {config.CustomParticleMaterial}");
        Log($"rotation {config.CustomParticleRotation}");
        Log($"rotation? {config.HasCustomRotation}");
        Footer(config);
    }

    public static void Dump(Gradient gradient) {
        Header(gradient);
        foreach(var colorKey in gradient.colorKeys) {
            Log($"color {colorKey.color} {colorKey.time}");
        }
        foreach(var alphaKey in gradient.alphaKeys) {
            Log($"alpha {alphaKey.alpha} {alphaKey.time}");
        }
        Footer(gradient);
    }
}
