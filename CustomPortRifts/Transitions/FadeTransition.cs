using UnityEngine;

namespace CustomPortRifts.Transitions;


public class FadeTransition(float startBeat, float duration) : Transition<float>(startBeat, duration) {
    public override float Interpolate(float t) => 1 - Mathf.Clamp01(Mathf.Abs(t - 0.5f) * 2);
}
