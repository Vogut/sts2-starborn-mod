using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.UI;

namespace STS2_Starborn.Patches;

public sealed class WidgetCombatUiReadyPatch : IPatchMethod
{
    private static readonly Vector2 KiboWidgetSize = new(132f, 192f);
    private static readonly Vector2 ElementMarkWidgetSize = new(56f, 118f);

    public static string PatchId => "sts2_starborn_widget_ready";
    public static string Description => "Inject Kibo and Element Mark widgets into NCombatUi";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NCombatUi), nameof(NCombatUi._Ready))];
    }

    public static void Postfix(NCombatUi __instance)
    {
        var kiboWidget = new NKiboWidget
        {
            Name = "NKiboWidget",
            Size = KiboWidgetSize,
        };
        __instance.AddChildSafely(kiboWidget);

        var elementWidget = new NElementMarkWidget
        {
            Name = "NElementMarkWidget",
            Size = ElementMarkWidgetSize,
        };
        __instance.AddChildSafely(elementWidget);
    }
}

public sealed class WidgetCombatUiActivatePatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_widget_activate";
    public static string Description => "Initialize Starborn widgets with player on combat activate";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NCombatUi), nameof(NCombatUi.Activate), [typeof(CombatState)])];
    }

    public static void Postfix(NCombatUi __instance, CombatState state)
    {
        var me = LocalContext.GetMe(state);
        if (me == null)
            return;

        foreach (var widget in __instance.GetChildren().OfType<NKiboWidget>())
        {
            widget.Initialize(me);
            StarbornCombatWidgetLayout.LayoutKibo(widget, me);
        }

        foreach (var widget in __instance.GetChildren().OfType<NElementMarkWidget>())
        {
            widget.Initialize(me);
            StarbornCombatWidgetLayout.LayoutElementMark(widget, me);
        }
    }
}
