using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace STS2_Starborn.Powers;

/// <summary>
/// 印记能力的抽象基类，继承自 <see cref="StarbornPower"/>。
/// 作为属性占位符：图标和描述随 <see cref="CurrentAttribute"/> 动态切换，
/// 触发效果委托给对应的 <see cref="ElementPower"/> 实例。
/// 印记最高 <see cref="MaxSealStacks"/> 层，达到 <see cref="ThresholdStacks"/> 层时触发基础效果，
/// 满层时触发强化效果，触发后层数减少但不低于 1。
/// </summary>
public abstract class SealMarkPower : StarbornPower
{
    /// <summary>最高印记层数</summary>
    public const int MaxSealStacks = 5;

    /// <summary>触发基础效果的最低层数</summary>
    public const int ThresholdStacks = 3;

    /// <summary>最低层数：保持能力存在（Amount=0 时能力会自动移除）</summary>
    private const int BaseSealAmount = 1;

    /// <summary>当前印记属性，回合开始时重置为无属性</summary>
    public SealAttribute CurrentAttribute { get; set; } = SealAttribute.None;

    /// <summary>当前激活的属性效果提供者；无属性时为 null</summary>
    private ElementPower? CurrentElement => ElementPower.For(CurrentAttribute);

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    // --- 动态图标：根据当前属性返回对应图标路径 ---

    /// <inheritdoc/>
    public override string? CustomIconPath =>
        $"res://STS2_Starborn/images/powers/{GetType().Name}_{CurrentAttribute}.png";

    /// <inheritdoc/>
    public override string? CustomBigIconPath => CustomIconPath;

    // --- 动态描述：属性激活时展示属性效果说明，否则展示通用描述 ---

    /// <inheritdoc/>
    public override LocString Description =>
        CurrentElement?.ElementDescription ?? base.Description;

    // --- 本地化变量：供通用描述中的 {MaxSealStacks} / {ThresholdStacks} 占位符使用 ---

    /// <inheritdoc/>
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("MaxSealStacks", MaxSealStacks),
        new DynamicVar("ThresholdStacks", ThresholdStacks),
    ];

    /// <summary>玩家回合开始时将属性重置为无属性</summary>
    public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side == Owner.Side)
            CurrentAttribute = SealAttribute.None;
        return Task.CompletedTask;
    }

    /// <summary>玩家回合结束时检查并触发印记效果，委托给当前 <see cref="ElementPower"/></summary>
    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != Owner.Side) return;

        var element = CurrentElement;

        if (Amount >= MaxSealStacks)
        {
            Flash();
            if (element != null)
                await element.OnEnhanced(choiceContext, this);
            SetAmount(Math.Max(BaseSealAmount, Amount - GetStacksToReduce(enhanced: true)));
        }
        else if (Amount >= ThresholdStacks)
        {
            Flash();
            if (element != null)
                await element.OnThreshold(choiceContext, this);
            SetAmount(Math.Max(BaseSealAmount, Amount - GetStacksToReduce(enhanced: false)));
        }
    }

    /// <summary>印记层数变化时检查上限，超过最大值则截断</summary>
    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && Amount > MaxSealStacks)
            SetAmount(MaxSealStacks);
        return Task.CompletedTask;
    }

    /// <summary>触发后消耗的印记层数；子类可按需重写</summary>
    protected virtual int GetStacksToReduce(bool enhanced) => enhanced ? 2 : 1;

    /// <summary>
    /// 切换当前印记属性。可在子类或外部卡牌中 await 调用，预留扩展空间（如未来的切换动画）。
    /// </summary>
    public virtual Task SetAttribute(PlayerChoiceContext ctx, SealAttribute attribute)
    {
        CurrentAttribute = attribute;
        return Task.CompletedTask;
    }
}
