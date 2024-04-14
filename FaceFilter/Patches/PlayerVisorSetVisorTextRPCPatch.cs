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
    [HarmonyPatch(nameof(PlayerVisor.RPCA_SetVisorText))]
    [HarmonyPostfix]
    private static void PreventPlebsFromUsingBadNames(PlayerVisor __instance)
    {
        var faceText = __instance.visorFaceText.text;
        if (!Filtration.IsProfane(faceText)) return;
        
        __instance.visorFaceText.text = FaceDatabase.GetFace(FaceDatabase.GetRandomFaceIndex());
        FaceFilter.Logger.LogInfo($"Blocking profane face text from '{__instance.m_player.name}': '{faceText}' -> " +
                                  $"'{__instance.visorFaceText.text}'");
        
        __instance.StartCoroutine(DelayedFaceTransmission(__instance, faceText));
    }

    private static IEnumerator DelayedFaceTransmission(PlayerVisor visor, string profanity)
    {
        var players = PlayerHandler.instance.GetPlayerID(Player.localPlayer);
        yield return new WaitForSeconds(0.25f*(players+1));
        if (visor.visorFaceText.text != profanity) yield break;
        visor.m_player.refs.view.RPC("RPCA_SetVisorText", RpcTarget.All, visor.visorFaceText.text);
    }
}