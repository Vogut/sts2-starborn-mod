using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Combat;
using STS2_Starborn.Hooks;
using STS2_Starborn.Element;

namespace STS2_Starborn.Commands;

public static class StarbornCmd
{
    public static bool CanTuning(Player player, MarkSlot slot)
    {
        var stacks = ElementMarkManager.GetStacks(player, slot);
        var elementType = ElementMarkManager.GetElementType(player, slot);
        if (elementType == SealElementType.None) return false;
        var consume = Element.Element.For(elementType).TuningConsume;
        if (player.Creature.CombatState != null)
            consume = SealElementMarkHooks.ModifyTuningConsume(player.Creature.CombatState, slot, consume);
        return stacks >= consume && consume >= 0;
    }

    public static bool CanOverload(Player player, MarkSlot slot)
    {
        var stacks = ElementMarkManager.GetStacks(player, slot);
        var elementType = ElementMarkManager.GetElementType(player, slot);
        if (elementType == SealElementType.None) return false;
        var consume = Element.Element.For(elementType).OverloadConsume;
        if (player.Creature.CombatState != null)
            consume = SealElementMarkHooks.ModifyOverloadConsume(player.Creature.CombatState, slot, consume);
        return stacks >= ElementMarkManager.ThresholdStacks && stacks >= consume && consume >= 0;
    }

    public static async Task Tuning(
        PlayerChoiceContext ctx,
        MarkSlot slot,
        Player player,
        int consume,
        CardModel? source = null)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var elementType = ElementMarkManager.GetElementType(player, slot);
        if (elementType == SealElementType.None) return;

        consume = SealElementMarkHooks.ModifyTuningConsume(combatState, slot, consume);
        var stacks = ElementMarkManager.GetStacks(player, slot);
        if (stacks < consume) return;

        await SealElementMarkHooks.BeforeTuning(combatState, ctx, slot, consume, source);
        if (consume > 0) await SealElementMarkCmd.RemoveElementMarks(ctx, slot, player, consume);
        await Element.Element.For(elementType).OnThreshold(ctx, player);
        await SealElementMarkHooks.AfterTuning(combatState, ctx, slot, consume, source);
    }

    public static async Task Tuning(
        PlayerChoiceContext ctx,
        MarkSlot slot,
        Player player,
        int consume,
        SealElementType targetElement,
        CardModel? source = null)
    {
        await SealElementMarkCmd.SetElementType(ctx, slot, player, targetElement);
        await Tuning(ctx, slot, player, consume, source);
    }
    /// <summary>
    /// 超限：消耗 <paramref name="consume"/> 枚印记，触发当前属性的强化效果。
    /// 需要印记层数 >= ThresholdStacks（3）。
    /// 卡牌上的"超限N"即调用 StarbornCmd.Overload(ctx, mark, N, owner, this)。
    /// </summary>
    public static async Task Overload(
        PlayerChoiceContext ctx,
        MarkSlot slot,
        Player player,
        int consume,
        CardModel? source = null)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var elementType = ElementMarkManager.GetElementType(player, slot);
        if (elementType == SealElementType.None) return;

        consume = SealElementMarkHooks.ModifyOverloadConsume(combatState, slot, consume);
        var stacks = ElementMarkManager.GetStacks(player, slot);
        if (stacks < ElementMarkManager.ThresholdStacks) return;
        if (stacks < consume) return;

        await SealElementMarkHooks.BeforeOverload(combatState, ctx, slot, consume, source);
        if (consume > 0) await SealElementMarkCmd.RemoveElementMarks(ctx, slot, player, consume);
        await Element.Element.For(elementType).OnEnhanced(ctx, player);
        await SealElementMarkHooks.AfterOverload(combatState, ctx, slot, consume, source);
    }

    public static async Task Overload(
        PlayerChoiceContext ctx,
        MarkSlot slot,
        Player player,
        int consume,
        SealElementType targetElement,
        CardModel? source = null)
    {
        await SealElementMarkCmd.SetElementType(ctx, slot, player, targetElement);
        await Overload(ctx, slot, player, consume, source);
    }
}
