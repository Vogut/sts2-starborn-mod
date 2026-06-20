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
    // 奇波控件默认尺寸。通常由 NKiboWidget 内部 DisplayScale 决定，这里只影响初始点击区域。
    private static readonly Vector2 KiboWidgetSize = new(132f, 192f);

    // 印记控件默认尺寸。高度 = 两个图标 + 间距；改 NElementMarkWidget 的 IconSize/Spacing 后同步这里。
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
    // 找不到角色节点时使用的兜底位置。只影响异常情况下的 UI 初始坐标。
    private static readonly Vector2 FallbackPlayerPosition = new(100f, 600f);

    // 找不到 hitbox 时使用的兜底尺寸。只影响异常情况下的左右下角计算。
    private static readonly Vector2 FallbackHitboxSize = new(240f, 220f);

    // 奇波相对人物右下角的横向偏移。数值越大，奇波越往右；数值越小，越靠近人物。
    private const float KiboOffsetX = 6f;

    // 奇波相对人物右下角的纵向偏移。数值越大，奇波越往下；数值越小，越往上。
    private const float KiboOffsetY = 24f;

    // 印记相对人物左下角的横向间距。数值越大，印记越往左；数值越小，越靠近人物。
    private const float ElementMarkOffsetX = 20f;

    // 印记相对人物左下角的纵向偏移。数值越大，印记越往下；数值越小，越往上。
    private const float ElementMarkOffsetY = 24f;

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
            : FallbackPlayerPosition;
        Vector2 hitboxLocalPos = creatureNode != null
            ? creatureNode.Hitbox.GlobalPosition - __instance.GlobalPosition
            : localPos;
        Vector2 hitboxSize = creatureNode?.Hitbox.Size ?? FallbackHitboxSize;
        Vector2 hitboxBottomLeft = hitboxLocalPos + new Vector2(0f, hitboxSize.Y);
        Vector2 hitboxBottomRight = hitboxLocalPos + hitboxSize;

        foreach (var widget in __instance.GetChildren().OfType<NKiboWidget>())
        {
            widget.Initialize(me);
            widget.Position = new Vector2(
                hitboxBottomRight.X + KiboOffsetX,
                hitboxBottomRight.Y - widget.Size.Y + KiboOffsetY);
            widget.UpdateFlightTargetPosition();
        }

        foreach (var widget in __instance.GetChildren().OfType<NElementMarkWidget>())
        {
            widget.Initialize(me);
            widget.Position = new Vector2(
                hitboxBottomLeft.X - widget.Size.X - ElementMarkOffsetX,
                hitboxBottomLeft.Y - widget.Size.Y + ElementMarkOffsetY);
        }
    }
}
