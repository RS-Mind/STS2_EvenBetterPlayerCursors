using BaseLib.Config;
using Godot;

namespace BetterPlayerCursors.BetterPlayerCursorsCode;

public class CursorConfig : SimpleModConfig
{
    // static required: SimpleModConfig only picks up static properties

    // own cursor follows your character's color during a run
    public static bool AutoCharacterColor { get; set; } = true;

    // multiplayer: assign colors by player slot instead of character
    public static bool RandomColors { get; set; } = false;

    // manual tint for your own cursor, used when AutoCharacterColor is off
    public static bool EnableTint { get; set; } = false;
    public static Color CursorColor { get; set; } = new(1f, 1f, 1f);

    public static bool ShowPlayerDots { get; set; } = false;
}