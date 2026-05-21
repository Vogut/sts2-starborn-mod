using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;

namespace STS2_Starborn.Hooks;

public interface ISealElementMarkListener
{
/// <summary>
/// 实现此接口的 AbstractModel（遗物、卡牌、能力等）可在印记属性变化时收到通知。
/// 在 CombatState 的监听者列表中自动参与分发（与原版 Hook 机制一致）。
/// </summary>
    Task BeforeElementChanged(PlayerChoiceContext ctx, MarkSlot slot, SealElementType from, SealElementType to)
        => Task.CompletedTask;

    Task AfterElementChanged(PlayerChoiceContext ctx, MarkSlot slot, SealElementType from, SealElementType to)
        => Task.CompletedTask;

    bool ShouldPreventElementChange(MarkSlot slot, SealElementType from, SealElementType to) => false;
}
