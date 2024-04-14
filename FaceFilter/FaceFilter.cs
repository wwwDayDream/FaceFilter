using System;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Mono.Cecil;
using ProfanityFilter;

namespace FaceFilter;

[ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, true)]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class FaceFilter : BaseUnityPlugin {
    public static FaceFilter Instance { get; private set; } = null!;
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static Harmony? Harmony { get; set; }

    private void Awake()
    {
        Logger = base.Logger;
        Instance = this;

        LoadProfanityFilter();
        Patch();

        Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
    }

    internal static void Patch()
    {
        Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

        Logger.LogDebug("Patching...");

        Harmony.PatchAll();

        Logger.LogDebug("Finished patching!");
    }

    private static void LoadProfanityFilter()
    {
        var location = Assembly.GetExecutingAssembly().Location;
        var profanityDll = location.Replace(Path.GetFileName(location), "ProfanityDetector.lib");
        if (!File.Exists(profanityDll))
            throw new Exception("No ProfanityDetector library found!");
        
        var assem = Assembly.LoadFile(profanityDll);
        if (assem == null)
            throw new Exception("Failed to load assembly!");
        
        Logger.LogDebug("Finished loading ProfanityDetector.lib!");
    }

    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}