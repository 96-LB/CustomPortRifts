using System.Collections.Generic;

namespace CustomPortRifts;


internal static class State<K, V> where V : new() {
    private static readonly Dictionary<K, V> states = [];

    internal static V Of(K obj) {
        if (!states.TryGetValue(obj, out V state)) {
            state = states[obj] = new();
        }
        
        return state;
    }
    internal static IEnumerable<(K, V)> All() {
        foreach(var pair in states) {
            yield return (pair.Key, pair.Value);
        }
    }
}
