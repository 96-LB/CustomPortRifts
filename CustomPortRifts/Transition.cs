using System;
using UnityEngine;

namespace CustomPortRifts;

public abstract class Transition<T>(float startBeat, float duration) {
    public T EndState => Interpolate(1);

    public float BeatToProgress(float beat) => beat < startBeat ? 0 : duration <= 0 || !float.IsFinite(duration) ? 1 : Mathf.Clamp01((beat - startBeat) / duration);
    public T Evaluate(float beat) => Interpolate(BeatToProgress(beat));
    public abstract T Interpolate(float t);
}

public class FadeTransition(float startBeat, float duration) : Transition<float>(startBeat, duration) {
    //public float FadeAmount(float t) => float.IsFinite(t) ? 1 - Mathf.Clamp01(Mathf.Abs(t - 0.5f) * 2 / fadeTime) : 1;
    //public float FadeAmountByBeat(float beat) => FadeAmount(BeatToProgress(beat));
    public override float Interpolate(float t) => 1 - Mathf.Clamp01(Mathf.Abs(t - 0.5f) * 2);
}

public class StaticTransition<T>(float startBeat, T value) : Transition<T>(startBeat, 0) {
    public override T Interpolate(float t) => value;

    public static StaticTransition<S> From<S>(float startBeat, S value) => new(startBeat, value);
}

public class TransitionManager<T> {
    public (Transition<T> Transition, Action<T> Callback)? Transition { get; private set; } = null;

    public void StartTransition(Transition<T> transition, Action<T> callback) {
        Update(float.PositiveInfinity); // immediately apply any pending transition
        Transition = (transition, callback);
    }

    public void Update(float beat) {
        if(Transition == null) {
            return;
        }

        var (transition, callback) = Transition.Value;
        var progress = transition.BeatToProgress(beat);
        if(progress > 0) {
            var state = transition.Interpolate(progress);
            callback?.Invoke(state);
            if(progress >= 1) {
                Transition = null;
            }
        }
    }
}
