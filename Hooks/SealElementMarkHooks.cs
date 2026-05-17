using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
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

    /// <summary>调谐消耗印记前广播。</summary>
    public static async Task BeforeTuning(
        ICombatState combatState,
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        Creature owner,
        CardModel? source)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is ITuningOverloadListener listener)
                await listener.BeforeTuning(ctx, mark, consume, owner, source);
            model.InvokeExecutionFinished();
        }
    }

    /// <summary>调谐效果完成后广播。</summary>
    public static async Task AfterTuning(
        ICombatState combatState,
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        Creature owner,
        CardModel? source)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is ITuningOverloadListener listener)
                await listener.AfterTuning(ctx, mark, consume, owner, source);
            model.InvokeExecutionFinished();
        }
    }

    /// <summary>超限消耗印记前广播。</summary>
    public static async Task BeforeOverload(
        ICombatState combatState,
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        Creature owner,
        CardModel? source)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is ITuningOverloadListener listener)
                await listener.BeforeOverload(ctx, mark, consume, owner, source);
            model.InvokeExecutionFinished();
        }
    }

    /// <summary>超限效果完成后广播。</summary>
    public static async Task AfterOverload(
        ICombatState combatState,
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        Creature owner,
        CardModel? source)
    {
        foreach (var model in combatState.IterateHookListeners())
        {
            if (model is ITuningOverloadListener listener)
                await listener.AfterOverload(ctx, mark, consume, owner, source);
            model.InvokeExecutionFinished();
        }
    }
}
