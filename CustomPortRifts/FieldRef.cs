using System.Reflection;

namespace CustomPortRifts;

internal class FieldRef<V>(object instance, FieldInfo field) {
    public V Value => (V)field.GetValue(instance);

    public FieldRef(object instance, string name) :
        this(instance, instance.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)) { }

    public FieldRef<V> Set(V value) {
        field.SetValue(instance, value);
        return this;
    }

    public override string ToString() {
        return $"[{instance}].{field.Name} = [{Value}]";
    }

    public static implicit operator V(FieldRef<V> fieldRef) {
        return fieldRef.Value;
    }
}
