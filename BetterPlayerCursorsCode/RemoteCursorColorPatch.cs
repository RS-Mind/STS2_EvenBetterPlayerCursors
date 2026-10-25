using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;

namespace EvenBetterPlayerCursors.EvenBetterPlayerCursorsCode;

// Modulate tints the whole cursor Control. Applied at _Ready for the initial color and
// re-applied on every UpdateImage so cursors created in the lobby (before RunState exists)
// pick up their real color once the run starts and input states change.
[HarmonyPatch]
public static class RemoteCursorColorPatches
{
    [HarmonyPatch(typeof(NRemoteMouseCursor), "_Ready")]
    [HarmonyPostfix]
    public static void ReadyPostfix(NRemoteMouseCursor __instance)
    {
        __instance.Modulate = PlayerColors.ForPlayer(__instance.PlayerId);
    }

    [HarmonyPatch(typeof(NRemoteMouseCursor), "UpdateImage")]
    [HarmonyPostfix]
    public static void UpdateImagePostfix(NRemoteMouseCursor __instance)
    {
        __instance.Modulate = PlayerColors.ForPlayer(__instance.PlayerId);
    }
}