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

public static class Filtration {
    public static ProfanityFilter.ProfanityFilter Filter { get; } = new();
    public static bool Initialized = false;

    public static bool IsProfane(string text)
    {
        if (!Initialized)
        {
            Initialized = true;
            Init();
        }
        return Filter.IsProfanity(text) || Filter.DetectAllProfanities(text).Count > 0 ||
               text.Aggregate(false, (b, c) => b || Filter.IsProfanity(c.ToString()));
    }

    public static void Init()
    {
        var resourceKey = typeof(FaceFilter).Namespace + ".WordBlock.txt";
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceKey);
        if (stream == null)
            throw new ArgumentException(resourceKey);
        using var wordStreamReader = new StreamReader(stream);

        while (!wordStreamReader.EndOfStream)
        {
            var word = wordStreamReader.ReadLine();
            Filter.AddProfanity(word);
        }
    }
}

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
    internal static void Unpatch()
    {
        Logger.LogDebug("Unpatching...");

        Harmony?.UnpatchSelf();

        Logger.LogDebug("Finished unpatching!");
    }
}