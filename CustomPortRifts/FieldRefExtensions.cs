namespace CustomPortRifts;


internal static class FieldRefExtensions {
    internal static FieldRef<V> Field<V>(this object instance, string name) {
        return new(instance, name);
    }
}
