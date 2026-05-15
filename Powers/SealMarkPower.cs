using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Scaffolding.Content;

namespace Starborn.Powers;

/// <summary>
/// 印记能力的抽象基类。印记最高5层，3层及以上回合结束时触发效果，5层触发强化效果。
/// 触发后印记层数减少但不低于1（保持能力在战场上存在）。
/// </summary>
public abstract class SealMarkPower : ModPowerTemplate
{
    /// <summary>最高印记层数</summary>
    public const int MaxSealStacks = 5;

    /// <summary>触发基础效果的最低层数</summary>
    public const int ThresholdStacks = 3;

    /// <summary>最低层数：保持能力存在（Amount=0 时能力会自动移除）</summary>
    private const int BaseSealAmount = 1;

    /// <summary>当前印记属性，回合开始时重置为无属性</summary>
    public SealAttribute CurrentAttribute { get; set; } = SealAttribute.None;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    /// <summary>玩家回合开始时将属性重置为无属性</summary>
    public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side == Owner.Side)
            CurrentAttribute = SealAttribute.None;
        return Task.CompletedTask;
    }

    /// <summary>玩家回合结束时检查并触发印记效果</summary>
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side) return;

        if (Amount >= MaxSealStacks)
        {
            Flash();
            await OnEnhancedEffect(choiceContext, CurrentAttribute);
            Amount = Math.Max(BaseSealAmount, Amount - GetStacksToReduce(enhanced: true));
        }
        else if (Amount >= ThresholdStacks)
        {
            Flash();
            await OnThresholdEffect(choiceContext, CurrentAttribute);
            Amount = Math.Max(BaseSealAmount, Amount - GetStacksToReduce(enhanced: false));
        }
    }

    /// <summary>印记层数变化时检查上限，超过最大值则截断</summary>
    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && Amount > MaxSealStacks)
            Amount = MaxSealStacks;
        return Task.CompletedTask;
    }

    /// <summary>基础效果（3层以上触发），由当前属性决定行为</summary>
    protected virtual async Task OnThresholdEffect(PlayerChoiceContext ctx, SealAttribute attribute)
    {
        switch (attribute)
        {
            case SealAttribute.Fire:
                await CreatureCmd.Damage(ctx, base.CombatState.HittableEnemies, 3m, ValueProp.Unpowered, base.Owner, null);
                break;
            case SealAttribute.Water:
                // TODO: 实现水属性基础效果
                break;
            case SealAttribute.Wood:
                // TODO: 实现木属性基础效果
                break;
        }
    }

    /// <summary>强化效果（5层触发），由当前属性决定行为</summary>
    protected virtual async Task OnEnhancedEffect(PlayerChoiceContext ctx, SealAttribute attribute)
    {
        switch (attribute)
        {
            case SealAttribute.Fire:
                await CreatureCmd.Damage(ctx, base.CombatState.HittableEnemies, 10m, ValueProp.Unpowered, base.Owner, null);
                break;
            case SealAttribute.Water:
                // TODO: 实现水属性强化效果
                break;
            case SealAttribute.Wood:
                // TODO: 实现木属性强化效果
                break;
        }
    }

    /// <summary>触发后消耗的印记层数；可按属性重写</summary>
    protected virtual int GetStacksToReduce(bool enhanced) => enhanced ? 2 : 1;
}
