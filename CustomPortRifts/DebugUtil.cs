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
}
