using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace EvenBetterPlayerCursors.EvenBetterPlayerCursorsCode;

// Small color marker above each player creature in multiplayer combat, matching their
// cursor color. Remote players' health bars are hidden by default, so the marker anchors
// to the always-visible Hitbox instead.
[HarmonyPatch(typeof(NCreature), "_Ready")]
public static class CreatureColorDotPatch
{
    private const float DotSize = 11f;

    public static void Postfix(NCreature __instance)
    {
        if (!CursorConfig.ShowPlayerDots)
        return;

        var player = __instance.Entity?.Player;
        if (player == null)
            return;

        if (__instance.Entity?.PetOwner != null)
            return; // pets/summons don't get their own marker

        // same multiplayer check the game uses for its intent handler
        var combatState = __instance.Entity?.CombatState;
        if (combatState == null || combatState.RunState.Players.Count <= 1)
            return;

        var dot = new ColorRect
        {
            Name = "PlayerColorDot",
            Color = PlayerColors.ForPlayer(player.NetId),
            Size = new Vector2(DotSize, DotSize),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        dot.Rotation = Mathf.Pi / 4f; // diamond reads less like a debug square
        dot.PivotOffset = new Vector2(DotSize / 2f, DotSize / 2f);

        var hitbox = __instance.Hitbox;
        dot.Position = new Vector2(hitbox.Size.X / 2f - DotSize / 2f, -DotSize * 1.5f);
        hitbox.AddChild(dot);
    }
}