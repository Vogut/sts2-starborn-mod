using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.UI;

namespace STS2_Starborn.Patches;

public sealed class WidgetCombatUiReadyPatch : IPatchMethod
{
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
            Size = new Vector2(220f, 140f),
        };
        __instance.AddChildSafely(kiboWidget);

        var elementWidget = new NElementMarkWidget
        {
            Name = "NElementMarkWidget",
            Size = new Vector2(56f, 100f),
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

        var creatureNode = NCombatRoom.Instance?.CreatureNodes
            .FirstOrDefault(n => n.Entity == me.Creature);
        Vector2 localPos = creatureNode != null
            ? creatureNode.GlobalPosition - __instance.GlobalPosition
            : new Vector2(100f, 600f);

        foreach (var widget in __instance.GetChildren().OfType<NKiboWidget>())
        {
            widget.Initialize(me);
            widget.Position = new Vector2(
                localPos.X + 100f,
                localPos.Y - widget.Size.Y + 40f);
        }

        foreach (var widget in __instance.GetChildren().OfType<NElementMarkWidget>())
        {
            widget.Initialize(me);
            widget.Position = new Vector2(
                localPos.X - 70f,
                localPos.Y - widget.Size.Y + 40f);
        }
    }
}
