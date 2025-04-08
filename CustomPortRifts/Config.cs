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
            Enabled = config.Bind("Enabled", true, "Enables custom portraits when a custom track supports them.");
        }
    }

    public static class CustomBackgrounds {
        public static ConfigEntry<bool> Colors { get; private set; }
        public static ConfigEntry<bool> Particles { get; private set; }
        public static void Initialize(ConfigGroup config) {
            Colors = config.Bind("Colors", true, "Enables custom background colors when a custom track supports them.");
            Particles = config.Bind("Particles", true, "Enables custom visualizer particles when athe custom track supports them.");
        }
    }

    public static void Initialize(ConfigFile config) {
        CustomPortraits.Initialize(new(config, "Custom Portraits"));
        CustomBackgrounds.Initialize(new(config, "Custom Backgrounds"));
    }
}
