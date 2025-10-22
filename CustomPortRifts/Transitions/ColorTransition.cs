using UnityEngine;

namespace CustomPortRifts.Transitions;


public class ColorTransition(Color color1, Color color2, float startBeat, float duration) : Transition<Color>(startBeat, duration) {
    Color Color1 { get; } = color1;
    Color Color2 { get; } = color2;

    public override Color Interpolate(float t) => Color.Lerp(Color1, Color2, t);
}
