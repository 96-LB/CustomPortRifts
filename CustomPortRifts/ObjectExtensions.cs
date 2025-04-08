using System;

namespace CustomPortRifts;


internal static class ObjectExtensions {
    public static void Pipe<T>(this T value, Action<T> func) {
        func(value);
    }

    public static R Pipe<T, R>(this T value, Func<T, R> func) {
        return func(value);
    }

    public static ref T Set<T>(this T value, ref T var) {
        var = value;
        return ref var;
    }
}
