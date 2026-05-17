using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Hooks;

/// <summary>
/// Starborn 印记系统的静态 Hook 分发器。
/// 仿照原版 Hook.cs 的 IterateHookListeners 模式，向所有监听者广播印记生命周期事件。
/// </summary>
public static class SealMarkHooks
{
    /// <summary>
    /// 当印记属性从 <paramref name="from"/> 切换为 <paramref name="to"/> 时触发。
    /// 通知战场上所有实现了 <see cref="ISealElementMarkListener"/> 的监听者
    /// （能力、遗物、卡牌、徽章等，与原版 Hook 分发范围一致）。
    /// </summary>
    public static async Task BeforeElementChanged(
        ICombatState combatState,
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        SealElementType from,
        SealElementType to)
    {
        foreach (var model in combatState.IterateHookListeners())
            {
                if (model is ISealElementMarkListener listener)
                    await listener.BeforeElementChanged(ctx, mark, from, to);

                model.InvokeExecutionFinished();
            }
    }

    public static async Task AfterElementChanged(
        ICombatState combatState,
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        SealElementType from,
        SealElementType to)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is ISealElementMarkListener listener)
                await listener.AfterElementChanged(ctx, mark, from, to);

            model.InvokeExecutionFinished();
        }
    }
}
