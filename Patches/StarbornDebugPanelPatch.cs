#if DEBUG
using MegaCrit.Sts2.Core.Nodes;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.RuntimeInput;
using STS2_Starborn.UI;

namespace STS2_Starborn.Patches;

/// <summary>
/// 在 NGame._Ready() 后将调试面板注入游戏根节点，并通过 F3 热键开关。
/// </summary>
public sealed class StarbornDebugPanelPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_debug_panel";
    public static string Description => "Inject StarbornDebugPanel into NGame and bind F3 hotkey";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NGame), nameof(NGame._Ready))];
    }

    public static void Postfix(NGame __instance)
    {
        var panel = new StarbornDebugPanel { Name = "StarbornDebugPanel" };
        __instance.AddChild(panel);

        RuntimeHotkeyService.Register("F3", panel.ToggleVisibility,
            new RuntimeHotkeyOptions
            {
                Id = "sts2_starborn.debug_panel",
                MarkInputHandled = true,
                DebugName = "Starborn Debug Panel",
            });
    }
}
#endif
