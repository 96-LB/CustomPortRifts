using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CustomPortRifts;


[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin {
    public const string GUID = "com.lalabuff.necrodancer.customportrifts";
    public const string NAME = "CustomPortRifts";
    public const string VERSION = "0.2.1";

    internal static ManualLogSource Log;

    internal void Awake() {
        Log = Logger;

        Harmony harmony = new(GUID);
        harmony.PatchAll();

        CustomPortRifts.Config.Initialize(Config);

        Log.LogInfo($"{NAME} v{VERSION} ({GUID}) has been loaded! Have fun!");
        foreach(var x in harmony.GetPatchedMethods()) {
            Log.LogInfo($"Patched {x}.");
        }
    }
}
