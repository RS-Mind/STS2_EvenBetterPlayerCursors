using System.Collections.Generic;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace BetterPlayerCursors.BetterPlayerCursorsCode;

public static class CursorTinter
{
    private static readonly string[] CursorFields = { "_cursorTilted", "_cursorNotTilted", "_cursorInspect" };

    // untinted originals, keyed by manager instance + field
    private static readonly Dictionary<(ulong, string), Image> Originals = new();

    private static NCursorManager? _manager;
    private static Color _appliedColor = new(1f, 1f, 1f);
    private static bool _tintedOnce;

    public static void OnCursorManagerSeen(NCursorManager instance)
    {
        _manager = instance;
        Apply(); // cheap no-op when the target color hasn't changed
    }

    public static void Apply()
    {
        if (_manager == null)
            return;

        Color target = TargetColor();
        if (target == _appliedColor && _tintedOnce)
            return;
        _tintedOnce = true;

        foreach (string field in CursorFields)
            TintField(_manager, field, target);

        _appliedColor = target;

        // clear the game's cursor cache so the new image actually gets applied
        AccessTools.Field(typeof(NCursorManager), "_lastSetCursor").SetValue(_manager, null);
        AccessTools.Method(typeof(NCursorManager), "UpdateCursor").Invoke(_manager, null);

        var inspect = (Image?)AccessTools.Field(typeof(NCursorManager), "_cursorInspect").GetValue(_manager);
        if (inspect != null)
            Input.SetCustomMouseCursor(inspect, Input.CursorShape.Help, new Vector2(12f, 12f)); // hotspot from decompile
    }

    private static Color TargetColor()
    {
        // in a run with auto on, follow the character (or slot, via ForPlayer's fallbacks)
        if (CursorConfig.AutoCharacterColor && LocalContext.NetId is ulong id)
        {
            Color c = PlayerColors.ForPlayer(id);
            if (c != new Color(1f, 1f, 1f) || !CursorConfig.EnableTint)
                return c;
        }

        return CursorConfig.EnableTint ? CursorConfig.CursorColor : new Color(1f, 1f, 1f);
    }

    private static void TintField(NCursorManager instance, string fieldName, Color tint)
    {
        var image = (Image?)AccessTools.Field(typeof(NCursorManager), fieldName).GetValue(instance);
        if (image == null)
            return;

        var key = (instance.GetInstanceId(), fieldName);
        if (!Originals.TryGetValue(key, out Image? original))
        {
            if (image.IsCompressed())
                image.Decompress();
            original = (Image)image.Duplicate();
            Originals[key] = original;
        }

        // rewrite from the clean original so repeated color changes don't stack
        for (int y = 0; y < image.GetHeight(); y++)
        {
            for (int x = 0; x < image.GetWidth(); x++)
            {
                Color p = original.GetPixel(x, y);
                float grayscale = (0.299f * p.R) + (0.587f * p.G) + (0.114f * p.B);
                image.SetPixel(x, y, new Color(grayscale * tint.R, grayscale * tint.G, grayscale * tint.B, p.A));
            }
        }
    }
}

// NCursorManager already exists before mods load, so _EnterTree alone never fires for it.
// UpdateCursor runs on every cursor refresh; Apply() early-outs when nothing changed, and
// this is also what picks up the character color once a run starts (no run-start event needed).
[HarmonyPatch(typeof(NCursorManager), "UpdateCursor")]
public static class UpdateCursorPatch
{
    public static void Prefix(NCursorManager __instance) => CursorTinter.OnCursorManagerSeen(__instance);
}

[HarmonyPatch(typeof(NCursorManager), "_EnterTree")]
public static class EnterTreePatch
{
    public static void Postfix(NCursorManager __instance) => CursorTinter.OnCursorManagerSeen(__instance);
}