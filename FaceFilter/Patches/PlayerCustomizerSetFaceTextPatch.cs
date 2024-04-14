using HarmonyLib;

namespace FaceFilter.Patches;

[HarmonyPatch(typeof(PlayerCustomizer))]
public static class PlayerCustomizerSetFaceTextPatch {

    [HarmonyPatch(nameof(PlayerCustomizer.SetFaceText))]
    [HarmonyPrefix]
    [HarmonyPriority(100)]
    private static void CensorFaceText(PlayerCustomizer __instance, ref string text)
    {
        if (!Filtration.IsProfane(text)) return;

        text = "***";
    }
}