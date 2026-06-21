using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace STS2_Starborn.UI;

internal static class StarbornCombatWidgetLayout
{
    private static readonly Vector2 FallbackPlayerPosition = new(100f, 600f);
    private static readonly Vector2 FallbackHitboxSize = new(240f, 220f);

    private const float KiboOffsetX = 6f;
    private const float KiboOffsetY = 24f;
    private const float ElementMarkOffsetX = 20f;
    private const float ElementMarkOffsetY = 24f;

    public static void LayoutKibo(NKiboWidget widget, Player? player)
    {
        if (player == null)
            return;

        if (!TryGetHitboxBounds(widget, player, out _, out var bottomRight))
            return;

        widget.Position = new Vector2(
            bottomRight.X + KiboOffsetX,
            bottomRight.Y - widget.Size.Y + KiboOffsetY);
        widget.UpdateFlightTargetPosition();
    }

    public static void LayoutElementMark(NElementMarkWidget widget, Player? player)
    {
        if (player == null)
            return;

        if (!TryGetHitboxBounds(widget, player, out var bottomLeft, out _))
            return;

        widget.Position = new Vector2(
            bottomLeft.X - widget.Size.X - ElementMarkOffsetX,
            bottomLeft.Y - widget.Size.Y + ElementMarkOffsetY);
    }

    private static bool TryGetHitboxBounds(
        Control widget,
        Player player,
        out Vector2 bottomLeft,
        out Vector2 bottomRight)
    {
        bottomLeft = Vector2.Zero;
        bottomRight = Vector2.Zero;

        if (widget.GetParent() is not Control parent)
            return false;

        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(player.Creature);

        if (creatureNode != null)
        {
            bottomLeft = ToParentLocal(
                parent,
                creatureNode.Hitbox,
                new Vector2(0f, creatureNode.Hitbox.Size.Y));
            bottomRight = ToParentLocal(parent, creatureNode.Hitbox, creatureNode.Hitbox.Size);
        }
        else
        {
            bottomLeft = FallbackPlayerPosition + new Vector2(0f, FallbackHitboxSize.Y);
            bottomRight = FallbackPlayerPosition + FallbackHitboxSize;
        }
        return true;
    }

    private static Vector2 ToParentLocal(Control parent, Control source, Vector2 sourceLocalPoint)
    {
        var globalPoint = source.GetGlobalTransformWithCanvas() * sourceLocalPoint;
        return parent.GetGlobalTransformWithCanvas().AffineInverse() * globalPoint;
    }
}
