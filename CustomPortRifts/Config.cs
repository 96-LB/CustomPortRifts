using BepInEx.Configuration;
using JetBrains.Annotations;

namespace CustomPortRifts;


public static class Config {
    public class Setting<T> {
        private ConfigEntry<T>? entry;
        public ConfigEntry<T> Entry => entry ?? throw new System.InvalidOperationException("Setting is not bound.");

        public static implicit operator T(Setting<T> setting) {
            return setting.Entry.Value;
        }
        
        public ConfigEntry<T> Bind(ConfigFile config, string group, string key, T defaultValue, string description) {
            return entry = config.Bind(group, key, defaultValue, description);
        }

        public ConfigEntry<T> Bind(ConfigFile config, string group, string key, T defaultValue, ConfigDescription? description = null) {
            return entry = config.Bind(group, key, defaultValue, description);
        }
    }

    public static class General {
        public static Setting<bool> Enabled { get; } = new();
        public static void Initialize(ConfigFile config, string group) {
            Enabled.Bind(config, group, "Enabled", true, "Enables the mod.");
        }
    }

    public static class Cadence {
        public static Setting<bool> Crypt { get; } = new();
        public static void Initialize(ConfigFile config, string group) {
            Crypt.Bind(config, group, "Crypt", false, "Enables the crypt costume for Cadence.");
        }
    }

    public static void Initialize(ConfigFile config) {
        General.Initialize(config, "General");
        Cadence.Initialize(config, "Cadence");
    }
}
