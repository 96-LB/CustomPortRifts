using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace CustomPortRifts;

public static class GradientUtil {
    [return: NotNullIfNotNull(nameof(a)), NotNullIfNotNull(nameof(b))]
    public static Gradient? Lerp(this Gradient? a, Gradient? b, float t) {
        if(b == null || t <= 0) return a;
        if(a == null || t >= 1) return b;
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

    [return: NotNullIfNotNull(nameof(a)), NotNullIfNotNull(nameof(b))]
    public static Gradient? Lerp(this Gradient? a, Color? b, float t) {
        if(b == null || t <= 0) return a;
        if(a == null || t >= 1) return new Gradient {
            colorKeys = [new GradientColorKey(b.Value, 0)],
            alphaKeys = [new GradientAlphaKey(b.Value.a, 0)]
        };
        return new Gradient {
            colorKeys = [.. a.colorKeys
                .Select(x => new GradientColorKey(Color.Lerp(x.color, b.Value, t), x.time))
            ],
            alphaKeys = [.. a.alphaKeys
                .Select(x => new GradientAlphaKey(Mathf.Lerp(x.alpha, b.Value.a, t), x.time))
            ],
            mode = a.mode
        };
    }
}
