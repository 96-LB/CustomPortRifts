using BepInEx.Configuration;

namespace CustomPortRifts;


public static class Config {
    public class ConfigGroup(ConfigFile config, string group) {
        public ConfigEntry<T> Bind<T>(string key, T defaultValue, string description) {
            return config.Bind(group, key, defaultValue, description);
        }

        public ConfigEntry<T> Bind<T>(string key, T defaultValue, ConfigDescription description = null) {
            return config.Bind(group, key, defaultValue, description);
        }
    }

    public static class General {
        public static ConfigEntry<bool> Enabled { get; private set; }
        public static void Initialize(ConfigGroup config) {
            Enabled = config.Bind("Enabled", true, "Enables the mod.");
        }
    }

    public static class Cadence {
        public static ConfigEntry<bool> Crypt { get; private set; }
        public static void Initialize(ConfigGroup config) {
            Crypt = config.Bind("Crypt", false, "Enables the crypt costume for Cadence.");
        }
    }

    public static class Characters {
        public static ConfigEntry<bool> Crypt { get; private set; }
        public static void Initialize(ConfigGroup config) {
            Crypt = config.Bind("Crypt", false, "Enables the crypt costume for Cadence.");
        }
    }

    public static void Initialize(ConfigFile config) {
        General.Initialize(new(config, "General"));
        Cadence.Initialize(new(config, "Cadence"));
    }
}
