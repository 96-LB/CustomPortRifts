using RiftOfTheNecroManager;

namespace CustomPortRifts;


public static class Config {
    public static class General {
        const string GROUP = "General";
        public static Setting<bool> TrackOverrides { get; } = new(GROUP, "Track Overrides", true, "Enables track-specific portrait overrides.");
        public static Setting<bool> CharacterOverrides { get; } = new(GROUP, "Character Overrides", true, "Enables character-specific portrait overrides.");
        public static Setting<bool> PortraitSwitching { get; } = new(GROUP, "Portrait Switching", true, "Allows custom tracks to switch character portraits in the middle of the level.");
        public static Setting<bool> VfxSwitching { get; } = new(GROUP, "VFX Switching", true, "Allows custom tracks to switch background visual effects in the middle of the level.");
    }

    public static class Reskins {
        const string GROUP = "Reskins";
        public static Setting<bool> CryptCadence { get; } = new(GROUP, "Crypt Cadence", false, "Enables the Crypt of the NecroDancer costume for Cadence on all tracks.");
        public static Setting<bool> CryptNecrodancer { get; } = new(GROUP, "Crypt NecroDancer", false, "Enables the Crypt of the NecroDancer costume for the NecroDancer on all tracks.");
        public static Setting<bool> Necroburger { get; } = new(GROUP, "Burger NecroDancer", false, "Enables the burger costume for the NecroDancer on all tracks.");
    }

    public static class ExtraModes {
        const string GROUP = "Extra Modes";
        public static Setting<bool> DisableBeastmaster { get; } = new(GROUP, "Disable Beastmaster", false, "Prevents Beastmaster from overriding the portrait when in Practice Mode.");
        public static Setting<bool> DisableCoda { get; } = new(GROUP, "Disable Coda", false, "Prevents Coda from overriding the portrait in Remix Mode and Coda Mode.");
        public static Setting<bool> DisableShopkeeper { get; } = new(GROUP, "Disable Shopkeeper", false, "Prevents Freddie from overriding the portrait in Shopkeeper Mode.");
    }
}
