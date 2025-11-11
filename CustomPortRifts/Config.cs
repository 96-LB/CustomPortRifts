using BepInEx.Configuration;

namespace CustomPortRifts;


public static class Config {
    public class Setting<T>(string key, T defaultValue, string description, AcceptableValueBase? acceptableValues = null, object[]? tags = null) {
        private ConfigEntry<T>? entry;
        public ConfigEntry<T> Entry => entry ?? throw new System.InvalidOperationException("Setting is not bound.");

        public static implicit operator T(Setting<T> setting) => setting.Entry.Value;
        public static implicit operator ConfigEntry<T>(Setting<T> setting) => setting.Entry;

        public ConfigEntry<T> Bind(ConfigFile config, string group) {
            return entry = config.Bind(group, key, defaultValue, new ConfigDescription(description, acceptableValues, tags));
        }
    }

    public static class General {
        const string GROUP = "General";

        public static Setting<bool> TrackOverrides { get; } = new("Track Overrides", true, "Enables track-specific portrait overrides.");
        public static Setting<bool> CharacterOverrides { get; } = new("Character Overrides", true, "Enables character-specific portrait overrides.");
        public static Setting<bool> PortraitSwitching { get; } = new("Portrait Switching", true, "Allows custom tracks to switch character portraits in the middle of the level.");
        public static Setting<bool> VfxSwitching { get; } = new("VFX Switching", true, "Allows custom tracks to switch background visual effects in the middle of the level.");


        public static void Bind(ConfigFile config) {
            TrackOverrides.Bind(config, GROUP);
            CharacterOverrides.Bind(config, GROUP);
            PortraitSwitching.Bind(config, GROUP);
            VfxSwitching.Bind(config, GROUP);
        }
    }

    public static class Reskins {
        const string GROUP = "Reskins";

        public static Setting<bool> CryptCadence { get; } = new("Crypt Cadence", false, "Enables the Crypt of the NecroDancer costume for Cadence on all tracks.");
        public static Setting<bool> CryptNecrodancer { get; } = new("Crypt NecroDancer", false, "Enables the Crypt of the NecroDancer costume for the NecroDancer on all tracks.");
        public static Setting<bool> Necroburger { get; } = new("Burger NecroDancer", false, "Enables the burger costume for the NecroDancer on all tracks.");

        public static void Bind(ConfigFile config) {
            CryptCadence.Bind(config, GROUP);
            CryptNecrodancer.Bind(config, GROUP);
            Necroburger.Bind(config, GROUP);
        }
    }

    public static class ExtraModes {
        const string GROUP = "Extra Modes";

        public static Setting<bool> DisableBeastmaster { get; } = new("Disable Beastmaster", false, "Prevents Beastmaster from overriding the portrait when in Practice Mode.");
        public static Setting<bool> DisableCoda { get; } = new("Disable Coda", false, "Prevents Coda from overriding the portrait in Remix Mode and Coda Mode.");
        public static Setting<bool> DisableShopkeeper { get; } = new("Disable Shopkeeper", false, "Prevents Freddie from overriding the portrait in Shopkeeper Mode.");

        public static void Bind(ConfigFile config) {
            DisableBeastmaster.Bind(config, GROUP);
            DisableCoda.Bind(config, GROUP);
            DisableShopkeeper.Bind(config, GROUP);
        }
    }
    
    public static class VersionControl {
        const string GROUP = "Version Control";

        public static Setting<string> VersionOverride { get; } = new("Version Override", "", "Input the current build version or '*' to override the version check.");

        public static void Bind(ConfigFile config) {
            VersionOverride.Bind(config, GROUP);
        }
    }

    public static void Bind(ConfigFile config) {
        General.Bind(config);
        Reskins.Bind(config);
        ExtraModes.Bind(config);
        VersionControl.Bind(config);
    }
}
