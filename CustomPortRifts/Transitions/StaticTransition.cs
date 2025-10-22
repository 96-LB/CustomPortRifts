namespace CustomPortRifts.Transitions;


public class StaticTransition<T>(float startBeat, T value) : Transition<T>(startBeat, 0) {
    public override T Interpolate(float t) => value;

    public static StaticTransition<S> From<S>(float startBeat, S value) => new(startBeat, value);
}
