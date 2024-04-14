using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;

namespace FaceFilter.Patches;

[HarmonyPatch(typeof(GameHandler))]
public static class GameHandlerCheckPluginsPatch {

    public static void RemoveProfanityDll(ref FileInfo[] files)
    {
        var filesAsList = files.ToList();
        var idx = -1;
        var assemLocation = typeof(ProfanityFilter.ProfanityFilter).Assembly.Location;
        if ((idx = filesAsList.FindIndex(info => info.FullName == assemLocation)) >= 0)
        {
            filesAsList.RemoveAt(idx);
            files = filesAsList.ToArray();
        }
    }
    [HarmonyPatch(nameof(GameHandler.CheckPlugins))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddExceptionForProfanityFilter(IEnumerable<CodeInstruction> instructions, ILGenerator ilGenerator)
    {
        return new CodeMatcher(instructions, ilGenerator)
            .MatchForward(true, [
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(OpCodes.Ldstr, "*.dll"),
                new CodeMatch(OpCodes.Ldc_I4_1),
                new CodeMatch(OpCodes.Callvirt),
                new CodeMatch(OpCodes.Stloc_2)
            ])
            .Advance(1)
            .Insert([
                new CodeInstruction(OpCodes.Ldloca_S, 2),
                new CodeInstruction(OpCodes.Call, typeof(GameHandlerCheckPluginsPatch).GetMethod(nameof(RemoveProfanityDll)))
            ])
            .ThrowIfInvalid("Couldn't find directoryInfo.GetFiles(...)")
            .Instructions();
    }
}