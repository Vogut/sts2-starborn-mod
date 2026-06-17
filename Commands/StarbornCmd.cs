using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2_Starborn.Combat;
using STS2_Starborn.Hooks;
using STS2_Starborn.Element;

namespace STS2_Starborn.Commands;

public static class StarbornCmd
{
    public static bool CanTuning(Player player, MarkSlot slot)
    {
        var stacks = ElementMarkState.GetStacks(player, slot);
        var elementType = ElementMarkState.GetElementType(player, slot);
        if (elementType == SealElementType.None) return false;
        var consume = Element.StarbornElement.For(elementType).TuningConsume;
        if (player.Creature.CombatState != null)
            consume = SealElementMarkHooks.ModifyTuningConsume(player.Creature.CombatState, slot, consume);
        return stacks >= consume && consume >= 0;
    }

    public static bool CanOverload(Player player, MarkSlot slot)
    {
        var stacks = ElementMarkState.GetStacks(player, slot);
        var elementType = ElementMarkState.GetElementType(player, slot);
        var consume = Element.StarbornElement.For(elementType).OverloadConsume;
        if (player.Creature.CombatState != null)
            consume = SealElementMarkHooks.ModifyOverloadConsume(player.Creature.CombatState, slot, consume);
        return stacks >= ElementMarkState.ThresholdStacks && stacks >= consume && consume >= 0;
    }

    public static async Task Tuning(
        PlayerChoiceContext ctx,
        MarkSlot slot,
        Player player,
        int consume,
        CardModel? source = null,
        IReadOnlyList<Creature>? targets = null)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var elementType = ElementMarkState.GetElementType(player, slot);
        if (elementType == SealElementType.None) return;

        // 卡牌触发的调谐：印记层数 > 3 时自动变为超限
        if (source != null)
        {
            var currentStacks = ElementMarkState.GetStacks(player, slot);
            if (currentStacks > ElementMarkState.ThresholdStacks)
            {
                var overloadConsume = Element.StarbornElement.For(elementType).OverloadConsume;
                await Overload(ctx, slot, player, overloadConsume, source, targets);
                return;
            }
        }

        consume = SealElementMarkHooks.ModifyTuningConsume(combatState, slot, consume);
        var stacks = ElementMarkState.GetStacks(player, slot);
        if (stacks < consume) return;

        await SealElementMarkHooks.BeforeTuning(combatState, ctx, slot, consume, source);
        if (consume > 0) await SealElementMarkCmd.RemoveElementMarks(ctx, slot, player, consume);
        var effectiveStacks = SealElementMarkHooks.ModifyEffectiveStacks(combatState, slot, stacks);
        await Element.StarbornElement.For(elementType).OnThreshold(ctx, player, effectiveStacks, source, targets);
        await SealElementMarkHooks.AfterTuning(combatState, ctx, slot, consume, source);
        ElementMarkManager.RecordTuning(player);
    }

    public static async Task Tuning(
        PlayerChoiceContext ctx,
        MarkSlot slot,
        Player player,
        int consume,
        SealElementType targetElement,
        CardModel? source = null,
        IReadOnlyList<Creature>? targets = null)
    {
        if (targetElement == SealElementType.None) return;
        await SealElementMarkCmd.SetElementType(ctx, slot, player, targetElement);
        await Tuning(ctx, slot, player, consume, source, targets);
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
        CardModel? source = null,
        IReadOnlyList<Creature>? targets = null)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var elementType = ElementMarkState.GetElementType(player, slot);
        if (elementType == SealElementType.None) return;

        consume = SealElementMarkHooks.ModifyOverloadConsume(combatState, slot, consume);
        var stacks = ElementMarkState.GetStacks(player, slot);
        if (stacks < ElementMarkState.ThresholdStacks) return;
        if (stacks < consume) return;

        await SealElementMarkHooks.BeforeOverload(combatState, ctx, slot, consume, source);
        if (consume > 0) await SealElementMarkCmd.RemoveElementMarks(ctx, slot, player, consume);
        var effectiveStacks = SealElementMarkHooks.ModifyEffectiveStacks(combatState, slot, stacks);
        await Element.StarbornElement.For(elementType).OnEnhanced(ctx, player, effectiveStacks, source, targets);
        await SealElementMarkHooks.AfterOverload(combatState, ctx, slot, consume, source);
        ElementMarkManager.RecordOverload(player);
    }

    public static async Task Overload(
        PlayerChoiceContext ctx,
        MarkSlot slot,
        Player player,
        int consume,
        SealElementType targetElement,
        CardModel? source = null,
        IReadOnlyList<Creature>? targets = null)
    {
        if (targetElement == SealElementType.None) return;
        await SealElementMarkCmd.SetElementType(ctx, slot, player, targetElement);
        await Overload(ctx, slot, player, consume, source, targets);
    }

    /// <summary>
    /// 弹跳：造成多次伤害，每次目标与上一次不同。
    /// 仅有一名敌人时退化：有效次数 = bounceCount - 2（最小 1）。
    /// 卡牌调用此重载，走 DamageCmd.Attack 触发攻击动画和卡牌Hook。
    /// </summary>
    public static async Task Bounce(
        PlayerChoiceContext ctx,
        Player player,
        CardModel card,
        decimal damagePerHit,
        int bounceCount)
    {
        await BounceCore(player, damagePerHit, bounceCount, async target =>
        {
            await DamageCmd.Attack(damagePerHit)
                .FromCard(card)
                .Targeting(target)
                .Execute(ctx);
        });
    }

    /// <summary>
    /// 弹跳：造成多次伤害，每次目标与上一次不同。
    /// 遗物/能力等非卡牌来源调用此重载，走 CreatureCmd.Damage 不触发攻击动画。
    /// </summary>
    public static async Task Bounce(
        Player player,
        Creature dealer,
        decimal damagePerHit,
        int bounceCount)
    {
        await BounceCore(player, damagePerHit, bounceCount, async target =>
        {
            await CreatureCmd.Damage(
                new BlockingPlayerChoiceContext(),
                [target],
                damagePerHit,
                ValueProp.Unpowered,
                dealer,
                cardSource: null);
        });
    }

    private static async Task BounceCore(
        Player player,
        decimal damagePerHit,
        int bounceCount,
        Func<Creature, Task> dealDamage)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var enemies = combatState.HittableEnemies.Where(e => e.IsAlive).ToList();
        if (enemies.Count == 0) return;

        var effectiveHits = enemies.Count == 1
            ? Math.Max(1, bounceCount - 2)
            : bounceCount;

        Creature? previous = null;
        for (var i = 0; i < effectiveHits; i++)
        {
            var candidates = enemies.Where(e => e != previous && e.IsAlive).ToList();
            var target = candidates.Count > 0
                ? candidates[Random.Shared.Next(candidates.Count)]
                : enemies.FirstOrDefault(e => e.IsAlive);
            if (target == null) break;

            await dealDamage(target);

            previous = target;
        }
    }
}
