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

    public static class CustomPortraits {
        public static ConfigEntry<bool> Enabled { get; private set; }
        public static void Initialize(ConfigGroup config) {
            Enabled = config.Bind("Enabled", true, "Enables custom portraits when the custom track supports them.");
        }
    }

    public static void Initialize(ConfigFile config) {
        CustomPortraits.Initialize(new(config, "Custom Portraits"));
    }
}
