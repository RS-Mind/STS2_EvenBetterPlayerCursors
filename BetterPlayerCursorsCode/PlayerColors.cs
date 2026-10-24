using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;

namespace BetterPlayerCursors.BetterPlayerCursorsCode;

public static class PlayerColors
{
    private static readonly Color[] Palette =
    {
        new(0.02f, 0.96f, 0.82f), // cyan
        new(0.94f, 0.16f, 0.04f), // red
        new(1.00f, 0.83f, 0.00f), // yellow
        new(0.86f, 0.03f, 0.99f), // magenta
        new(0.13f, 0.98f, 0.00f), // green
        new(0.97f, 0.56f, 0.02f), // orange
        new(0.14f, 0.40f, 1.00f), // blue
        new(1.00f, 0.56f, 0.67f), // pink
    };

    public static Color ForPlayer(ulong netId)
    {
        RunState? state = GetRunState();

        if (!CursorConfig.RandomColors && state != null)
        {
            Color? charColor = CharacterColorFor(state, netId);
            if (charColor != null)
                return charColor.Value;
        }

        int slot = state?.GetPlayerSlotIndex(netId) ?? -1;
        if (slot >= 0)
            return Palette[slot % Palette.Length];

        // outside a run: stable hash fallback, collisions possible
        ulong h = netId * 2654435761UL;
        return Palette[(int)(h % (ulong)Palette.Length)];
    }

    private static Color? CharacterColorFor(RunState state, ulong netId)
    {
        Player? player = state.GetPlayer(netId);
        if (player?.Character == null)
            return null;

        Color? baseColor = player.Character.NameColor;

        // same character picked twice: later slots get progressively lighter shades
        int duplicateRank = 0;
        int mySlot = state.GetPlayerSlotIndex(netId);
        foreach (Player other in state.Players)
        {
            if (other.NetId == netId || other.Character == null)
                continue;
            if (other.Character.GetType() == player.Character.GetType()
                && state.GetPlayerSlotIndex(other) < mySlot)
                duplicateRank++;
        }

        return duplicateRank == 0
            ? baseColor.Value
            : baseColor.Value.Lightened(Mathf.Min(0.25f * duplicateRank, 0.75f));
    }

    private static RunState? GetRunState()
    {
        // RunManager.State is private; DebugOnlyGetState is marked tests-only,
        // so reflection on the property is the safer long-term bet
        return (RunState?)AccessTools.Property(typeof(RunManager), "State")
            .GetValue(RunManager.Instance);
    }
}