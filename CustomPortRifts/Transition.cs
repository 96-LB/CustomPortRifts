using CustomPortRifts.Patches;
using UnityEngine;

namespace CustomPortRifts;

public abstract class Transition<T>(float startBeat, float duration) {
    public T EndState => Interpolate(1);

    public float BeatToProgress(float beat) => duration <= 0 || !float.IsFinite(duration) ? 1 : Mathf.Clamp01((beat - startBeat) / duration);
    public T Evaluate(float beat) => Interpolate(BeatToProgress(beat));
    public abstract T Interpolate(float t);
}

public abstract class FadeTransition<T>(float startBeat, float duration, float fadeTime) : Transition<T>(startBeat, duration) {
    public float FadeAmount(float t) => float.IsFinite(t) ? 1 - Mathf.Clamp01(Mathf.Abs(t - 0.5f) * 2 / fadeTime) : 1;
    public float FadeAmountByBeat(float beat) => FadeAmount(BeatToProgress(beat));
}
