using UnityEngine;

namespace CustomPortRifts.Transitions;


public abstract class Transition<T>(float startBeat, float duration) {
    public T EndState => Interpolate(1);

    public float BeatToProgress(float beat) => beat < startBeat ? 0 : duration <= 0 || !float.IsFinite(duration) ? 1 : Mathf.Clamp01((beat - startBeat) / duration);
    public T Evaluate(float beat) => Interpolate(BeatToProgress(beat));
    public abstract T Interpolate(float t);
}
