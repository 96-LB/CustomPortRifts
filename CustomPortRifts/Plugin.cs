﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Shared;
using System;
using System.Linq;

namespace CustomPortRifts;


[BepInPlugin(GUID, NAME, VERSION)]
public class Plugin : BaseUnityPlugin {
    public const string GUID = "com.lalabuff.necrodancer.customportrifts";
    public const string NAME = "CustomPortRifts";
    public const string VERSION = "1.0.0";

    public const string ALLOWED_VERSIONS = "1.6.0";
    public static string[] AllowedVersions => ALLOWED_VERSIONS.Split(' ');

    internal static ManualLogSource Log { get; private set; } = new(NAME);

    internal void Awake() {
        try {
            Log = Logger;

            CustomPortRifts.Config.Bind(Config);

            var gameVersion = BuildInfoHelper.Instance.BuildId.Split('-')[0];
            var overrideVersion = CustomPortRifts.Config.VersionControl.VersionOverride;
            var check = AllowedVersions.Contains(gameVersion) || gameVersion == overrideVersion || overrideVersion == "*";
            if(!check) {
                Log.LogFatal($"The current version of the game is not compatible with this plugin. Please update the game or the mod to the correct version. The current mod version is v{VERSION} and the current game version is {gameVersion}. Allowed game versions: {string.Join(", ", AllowedVersions)}");
                return;
            }

            Harmony harmony = new(GUID);
            harmony.PatchAll();
            foreach(var x in harmony.GetPatchedMethods()) {
                Log.LogInfo($"Patched {x}.");
            }
            Log.LogMessage($"{NAME} v{VERSION} ({GUID}) has been loaded!");
        } catch(Exception e) {
            Log.LogFatal("Encountered error while trying to initialize plugin.");
            Log.LogFatal(e);
        }
    }
}
