using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Hooks;

/// <summary>
/// 实现此接口的 AbstractModel（遗物、卡牌、能力等）可在印记属性变化时收到通知。
/// 在 CombatState 的监听者列表中自动参与分发（与原版 Hook 机制一致）。
/// </summary>
public interface ISealElementMarkListener
{
    /// <summary>
    /// 当印记属性从 <paramref name="from"/> 切换为 <paramref name="to"/> 时被调用。
    /// </summary>
    /// <param name="ctx">当前玩家操作上下文。</param>
    /// <param name="mark">触发属性切换的印记能力实例。</param>
    /// <param name="from">切换前的属性。</param>
    /// <param name="to">切换后的属性。</param>
    Task BeforeElementChanged(PlayerChoiceContext ctx, SealElementMarkPower mark, SealElementType from, SealElementType to)        
    {
        return Task.CompletedTask;
    }
    Task AfterElementChanged(PlayerChoiceContext ctx, SealElementMarkPower mark, SealElementType from, SealElementType to)
    {
        return Task.CompletedTask;
    }
}
