using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Patching.Models;
using STS2_Starborn.Kibo;

namespace STS2_Starborn.Patches;

public sealed class KiboWidgetCombatUiReadyPatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_kibo_widget_ready";
    public static string Description => "Inject Kibo widget into NCombatUi";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets()
    {
        return [new(typeof(NCombatUi), nameof(NCombatUi._Ready))];
    }

    public static void Postfix(NCombatUi __instance)
    {
        var widget = new NKiboWidget
        {
            Name = "NKiboWidget",
            Size = new Vector2(220f, 140f),
        };
        __instance.AddChildSafely(widget);
    }
}

public sealed class KiboWidgetCombatUiActivatePatch : IPatchMethod
{
    public static string PatchId => "sts2_starborn_kibo_widget_activate";
    public static string Description => "Initialize Kibo widget with player on combat activate";
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

            var creatureNode = NCombatRoom.Instance?.CreatureNodes
                .FirstOrDefault(n => n.Entity == me.Creature);
            if (creatureNode != null)
            {
                var localPos = creatureNode.GlobalPosition - __instance.GlobalPosition;
                widget.Position = new Vector2(
                    localPos.X + 100f,
                    localPos.Y - widget.Size.Y + 40f);
            }
        }
    }
}
