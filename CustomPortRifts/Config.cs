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

    public static class Custom {
        public static ConfigEntry<bool> Portraits { get; private set; }
        public static ConfigEntry<bool> Colors { get; private set; }
        public static ConfigEntry<bool> Particles { get; private set; }
        public static void Initialize(ConfigGroup config) {
            Portraits = config.Bind("Custom Portraits", true, "Enables custom portraits when a custom track supports them.");
            Colors = config.Bind("Custom Colors", true, "Enables custom background colors when a custom track supports them.");
            Particles = config.Bind("Custom Particles", true, "Enables custom visualizer particles when athe custom track supports them.");
        }
    }

    public static class PracticeMode {
        public static ConfigEntry<bool> Portraits { get; private set; }
        public static ConfigEntry<bool> Colors { get; private set; }
        public static ConfigEntry<bool> Particles { get; private set; }
        public static void Initialize(ConfigGroup config) {
            Portraits = config.Bind("Custom Portraits", false, "Enables custom portraits in practice mode.");
            Colors = config.Bind("Custom Colors", false, "Enables custom background colors in practice mode.");
            Particles = config.Bind("Custom Particles", false, "Enables custom particles in practice mode.");
        }
    }
    
    public static void Initialize(ConfigFile config) {
        General.Initialize(new(config, "General"));
        Custom.Initialize(new(config, "Custom Portraits"));
        PracticeMode.Initialize(new(config, "Practice Mode"));
    }
}
