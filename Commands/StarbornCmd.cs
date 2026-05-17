using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Hooks;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Commands;

public static class StarbornCmd
{
    public static bool CanTuning(SealElementMarkPower? mark, int consume)
        => mark?.CanTuning(consume) ?? false;

    public static bool CanOverload(SealElementMarkPower? mark, int consume)
        => mark?.CanOverload(consume) ?? false;

    public static async Task Tuning(
        PlayerChoiceContext ctx,
        SealElementMarkPower mark,
        int consume,
        CardModel? source = null)
    {
        if (consume <= 0) return;
        if (mark.DisplayAmount < consume) return;

        await SealElementMarkHooks.BeforeTuning(mark.CombatState, ctx, mark, consume, source);
        await SealElementMarkCmd.RemoveElementMarks(ctx, mark, consume, source);
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
        if (consume <= 0) return;
        if (mark.DisplayAmount < SealElementMarkPower.ThresholdStacks) return;
        if (mark.DisplayAmount < consume) return;

        await SealElementMarkHooks.BeforeTuning(mark.CombatState, ctx, mark, consume, source);
        await SealElementMarkHooks.BeforeOverload(mark.CombatState, ctx, mark, consume, source);
        await SealElementMarkCmd.RemoveElementMarks(ctx, mark, consume, source);
        await mark.TriggerOverload(ctx);
        await SealElementMarkHooks.AfterTuning(mark.CombatState, ctx, mark, consume, source);
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
