using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace CustomPortRifts;

public static class GradientUtil {
    [return: NotNullIfNotNull(nameof(a)), NotNullIfNotNull(nameof(b))]
    public static Gradient? Lerp(Gradient? a, Gradient? b, float t) {
        if(a == null) return b;
        if(b == null) return a;
        if(t <= 0) return a;
        if(t >= 1) return b;
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
