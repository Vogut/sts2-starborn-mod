using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Hooks;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Commands;

/// <summary>
/// Starborn 模组级统一命令入口，负责跨系统的通用操作（调谐、超限等）。
/// 具体印记层数操作委托给 <see cref="SealElementMarkCmd"/>，
/// 元素效果触发委托给 <see cref="SealElementMarkPower"/> 自身。
/// </summary>
public static class StarbornCmd
{
    /// <summary>
    /// 调谐：消耗 <paramref name="consume"/> 枚印记，触发当前属性的阈值效果。
    /// 卡牌上的"调谐N"即调用 StarbornCmd.Tuning(ctx, mark, N, owner, this)。
    /// </summary>
    public static async Task Tuning(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        Creature owner,
        CardModel? source)
    {
        if (consume <= 0) return;

        await SealElementMarkHooks.BeforeTuning(mark.CombatState, ctx, mark, consume, owner, source);
        await SealElementMarkCmd.RemoveElementMarks(ctx, mark, consume, owner, source);
        await mark.TriggerTuning(ctx);
        await SealElementMarkHooks.AfterTuning(mark.CombatState, ctx, mark, consume, owner, source);
    }

    /// <summary>
    /// 超限：消耗 <paramref name="consume"/> 枚印记，触发当前属性的强化效果。
    /// 需要印记层数 >= ThresholdStacks（3）。
    /// 卡牌上的"超限N"即调用 StarbornCmd.Overload(ctx, mark, N, owner, this)。
    /// </summary>
    public static async Task Overload(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        Creature owner,
        CardModel? source)
    {
        if (consume <= 0) return;
        if (mark.DisplayAmount < SealElementMarkPower.ThresholdStacks) return;
        /// 超限效果触发前后都广播调谐事件，确保所有监听者能正确响应属性变化和层数消耗。
        await SealElementMarkHooks.AfterTuning(mark.CombatState, ctx, mark, consume, owner, source);
        await SealElementMarkHooks.BeforeOverload(mark.CombatState, ctx, mark, consume, owner, source);
        await SealElementMarkCmd.RemoveElementMarks(ctx, mark, consume, owner, source);
        await mark.TriggerOverload(ctx);
        await SealElementMarkHooks.AfterTuning(mark.CombatState, ctx, mark, consume, owner, source);
        await SealElementMarkHooks.AfterOverload(mark.CombatState, ctx, mark, consume, owner, source);
    }
}
