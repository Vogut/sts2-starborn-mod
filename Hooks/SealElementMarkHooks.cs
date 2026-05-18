using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Hooks;

/// <summary>
/// Starborn 印记系统的静态 Hook 分发器。
/// 仿照原版 Hook.cs 的 IterateHookListeners 模式，向所有监听者广播印记生命周期事件。
/// </summary>
public static class SealElementMarkHooks
{
    /// <summary>
    /// 当印记属性从 <paramref name="from"/> 切换为 <paramref name="to"/> 时触发。
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

    public static async Task BeforeTuning(
        ICombatState combatState,
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        CardModel? source)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is ITuningOverloadListener listener)
                await listener.BeforeTuning(ctx, mark, consume, source);
            model.InvokeExecutionFinished();
        }
    }

    public static async Task AfterTuning(
        ICombatState combatState,
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        CardModel? source)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is ITuningOverloadListener listener)
                await listener.AfterTuning(ctx, mark, consume, source);
            model.InvokeExecutionFinished();
        }
    }

    public static async Task BeforeOverload(
        ICombatState combatState,
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        CardModel? source)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is ITuningOverloadListener listener)
                await listener.BeforeOverload(ctx, mark, consume, source);
            model.InvokeExecutionFinished();
        }
    }

    public static async Task AfterOverload(
        ICombatState combatState,
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        CardModel? source)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is ITuningOverloadListener listener)
                await listener.AfterOverload(ctx, mark, consume, source);
            model.InvokeExecutionFinished();
        }
    }

    /// <summary>
    /// 向所有 <see cref="IConsumeModifier"/> 分发调谐消耗修正。
    /// 对标原版 <c>Hook.ModifyDamage</c> 模式。
    /// </summary>
    public static int ModifyTuningConsume(ICombatState combatState, SealElementMarkPower mark, int consume)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IConsumeModifier modifier)
                consume = modifier.ModifyTuningConsume(mark, consume);
        }
        return Math.Max(0, consume);
    }

    /// <summary>
    /// 向所有 <see cref="IConsumeModifier"/> 分发超限消耗修正。
    /// </summary>
    public static int ModifyOverloadConsume(ICombatState combatState, SealElementMarkPower mark, int consume)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is IConsumeModifier modifier)
                consume = modifier.ModifyOverloadConsume(mark, consume);
        }
        return Math.Max(0, consume);
    }

}
