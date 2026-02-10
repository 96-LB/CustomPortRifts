using BepInEx;
using RiftOfTheNecroManager;

namespace CustomPortRifts;


[BepInPlugin(GUID, NAME, VERSION)]
[NecroManagerInfo(menuNameOverride: "Custom PortRifts", isBeta: true)]
public class Plugin : RiftPlugin {
    public const string GUID = "com.lalabuff.necrodancer.customportrifts";
    public const string NAME = "CustomPortRifts";
    public const string VERSION = "2.0.0";
}
