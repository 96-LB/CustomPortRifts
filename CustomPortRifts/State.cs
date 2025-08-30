using System.Collections.Generic;

namespace CustomPortRifts;


public abstract class State<K, V> where V : State<K, V>, new() {
    private static readonly Dictionary<K, V> states = [];

    public K Instance { get; private set; } = default!; // always construct with .Of or this can cause null refs

    protected State() { }

    public static V Of(K obj) {
        if(!states.TryGetValue(obj, out V state)) {
            state = states[obj] = new();
            state.Instance = obj;
        }

        return state;
    }

    public static IEnumerable<(K, V)> All() {
        foreach(var pair in states) {
            yield return (pair.Key, pair.Value);
        }
    }
}
