using Shared;
using System.Linq;
using UnityEngine;

namespace CustomPortRifts;

public static class GradientUtil {
    public static Gradient Lerp(Gradient a, Gradient b, float t) {
        return new Gradient {
            colorKeys = [.. a.colorKeys.Concat(b.colorKeys)
                .Select(x => x.time)
                .Distinct()
                .Select(x => new GradientColorKey(Color.Lerp(a.Evaluate(x), b.Evaluate(x), t), x))
            ],
            alphaKeys = [.. a.alphaKeys.Concat(b.alphaKeys)
                .Select(x => x.time)
                .Distinct()
                .Select(x => new GradientAlphaKey(Color.Lerp(a.Evaluate(x), b.Evaluate(x), t).a, x))
            ],
            mode = a.mode
        };
    }
}
