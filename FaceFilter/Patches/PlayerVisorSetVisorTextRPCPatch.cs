using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace FaceFilter.Patches;

[HarmonyPatch(typeof(PlayerVisor))]
public static class PlayerVisorSetVisorTextRPCPatch {
    public static ProfanityFilter.ProfanityFilter Filter { get; } = new();

    static PlayerVisorSetVisorTextRPCPatch()
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

    private static bool IsProfane(string text) =>
        Filter.IsProfanity(text) || Filter.DetectAllProfanities(text).Count > 0 ||
        text.Aggregate(false, (b, c) => b || Filter.IsProfanity(c.ToString()));
    
    [HarmonyPatch(nameof(PlayerVisor.RPCA_SetVisorText))]
    [HarmonyPostfix]
    private static void PreventPlebsFromUsingBadNames(PlayerVisor __instance)
    {
        
        var faceText = __instance.visorFaceText.text;
        if (!IsProfane(faceText)) return;
        
        __instance.visorFaceText.text = FaceDatabase.GetFace(FaceDatabase.GetRandomFaceIndex());
        FaceFilter.Logger.LogInfo($"Blocking profane face text from '{__instance.m_player.name}': '{faceText}' -> " +
                                  $"'{__instance.visorFaceText.text}'");
        
        if (!Player.localPlayer.refs.view.Controller.IsMasterClient) return;
        __instance.StartCoroutine(DelayedFaceTransmission(__instance));
    }

    private static IEnumerator DelayedFaceTransmission(PlayerVisor visor)
    {
        yield return new WaitForSeconds(0.5f);
        visor.m_player.refs.view.RPC("RPCA_SetVisorText", RpcTarget.All, visor.visorFaceText.text);
    }
}