using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Hooks;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Commands;

public static class StarbornCmd
{
    public static bool CanTuning(SealElementMarkPower? mark, int consume)
    {
        if (mark == null) return false;
        consume = SealElementMarkHooks.ModifyTuningConsume(mark.CombatState, mark, consume);
        return mark.CanTuning(consume);
    }

    public static bool CanOverload(SealElementMarkPower? mark, int consume)
    {
        if (mark == null) return false;
        consume = SealElementMarkHooks.ModifyOverloadConsume(mark.CombatState, mark, consume);
        return mark.CanOverload(consume);
    }

    public static async Task Tuning(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        CardModel? source = null)
    {
        consume = SealElementMarkHooks.ModifyTuningConsume(mark.CombatState, mark, consume);
        if (mark.DisplayAmount < consume) return;

        await SealElementMarkHooks.BeforeTuning(mark.CombatState, ctx, mark, consume, source);
        if (consume > 0) await SealElementMarkCmd.RemoveElementMarks(ctx, mark, consume, source);
        await mark.TriggerTuning(ctx);
        await SealElementMarkHooks.AfterTuning(mark.CombatState, ctx, mark, consume, source);
    }

    public static async Task Tuning(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        SealElementType targetElement,
        CardModel? source = null)
    {
        await SealElementMarkCmd.SetElementType(ctx, mark, targetElement);
        await Tuning(ctx, mark, consume, source);
    }

    public static async Task Overload(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        CardModel? source = null)
    {
        consume = SealElementMarkHooks.ModifyOverloadConsume(mark.CombatState, mark, consume);
        if (mark.DisplayAmount < SealElementMarkPower.ThresholdStacks) return;
        if (mark.DisplayAmount < consume) return;

        await SealElementMarkHooks.BeforeOverload(mark.CombatState, ctx, mark, consume, source);
        if (consume > 0) await SealElementMarkCmd.RemoveElementMarks(ctx, mark, consume, source);
        await mark.TriggerOverload(ctx);
        await SealElementMarkHooks.AfterOverload(mark.CombatState, ctx, mark, consume, source);
    }

    public static async Task Overload(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        SealElementType targetElement,
        CardModel? source = null)
    {
        await SealElementMarkCmd.SetElementType(ctx, mark, targetElement);
        await Overload(ctx, mark, consume, source);
    }
}
