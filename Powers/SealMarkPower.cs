using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace STS2_Starborn.Powers;

/// <summary>
/// 失序印记的抽象基类，继承自 <see cref="StarbornPower"/>。
/// 作为属性占位符：图标和描述随 <see cref="CurrentAttribute"/> 动态切换，
/// 触发效果委托给对应的 <see cref="ElementPower"/> 实例。
/// 印记最高 <see cref="MaxSealStacks"/> 层（显示 0~5），
/// 达到 <see cref="ThresholdStacks"/> 层时触发基础效果，满层触发强化效果。
/// 内部 Amount 始终 = stacks + 1，防止 Amount=0 时 power 被框架自动移除。
/// </summary>
public abstract class SealMarkPower : StarbornPower
{
    private class Data
    {
        public int stacks;
    }

    /// <summary>最高显示层数</summary>
    public const int MaxSealStacks = 5;

    /// <summary>触发基础效果的最低显示层数</summary>
    public const int ThresholdStacks = 3;

    /// <summary>当前印记属性，回合开始时重置为无属性</summary>
    public SealAttribute CurrentAttribute { get; set; } = SealAttribute.None;

    /// <summary>当前激活的属性效果提供者；无属性时返回 <see cref="NonElementPower"/></summary>
    private ElementPower CurrentElement => ElementPower.For(CurrentAttribute);

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    /// <summary>UI 显示的层数（0~<see cref="MaxSealStacks"/>），直接读取内部数据，独立于 Amount</summary>
    public override int DisplayAmount => GetInternalData<Data>().stacks;

    // --- 动态图标：根据当前属性返回对应图标路径 ---
    public override string? CustomIconPath =>
        $"res://STS2_Starborn/powers/Elements/{CurrentAttribute}.png";
    public override string? CustomBigIconPath => CustomIconPath;

    // --- 动态标题：根据当前属性返回对应 title ---
    public override LocString Title =>
        new LocString("powers", $"STS2_STARBORN_ELEMENT_{CurrentAttribute.ToString().ToUpperInvariant()}.title");

    // --- 动态描述：SmartDescriptionLocKey 动态返回当前属性对应的 key ---
    protected override string SmartDescriptionLocKey =>
        $"STS2_STARBORN_ELEMENT_{CurrentAttribute.ToString().ToUpperInvariant()}.smartDescription";
    public override LocString Description => CurrentElement.ElementDescription;

    // --- 本地化变量：{MaxSealStacks}/{ThresholdStacks} 为常量，{Stacks} 跟踪当前层数（非 Amount）---
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("MaxSealStacks", MaxSealStacks),
        new DynamicVar("ThresholdStacks", ThresholdStacks),
        new DynamicVar("Stacks", 0),
        
    ];

    /// <inheritdoc/>
    protected override object InitInternalData() => new Data();

    /// <summary>
    /// 设置印记层数（0~<see cref="MaxSealStacks"/>）。
    /// 同步 Amount = stacks + 1（保活）并通知 UI 刷新显示数字。
    /// </summary>
    protected void SetStacks(int stacks)
    {
        var data = GetInternalData<Data>();
        data.stacks = Math.Clamp(stacks, 0, MaxSealStacks);
        DynamicVars["Stacks"].BaseValue = data.stacks;
        SetAmount(data.stacks + 1); // Amount 始终 ≥ 1，防止 power 被移除
        InvokeDisplayAmountChanged();
    }

    /// <summary>
    /// 外部（卡牌/框架）直接修改 Amount 时，同步内部 stacks 并修正 Amount。
    /// 采用 OrbitPower 模式：检查 Amount 是否已正确，避免无限递归。
    /// </summary>
    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this)
        {
            var data = GetInternalData<Data>();
            data.stacks = Math.Clamp((int)Amount - 1, 0, MaxSealStacks);
            DynamicVars["Stacks"].BaseValue = data.stacks;
            var correct = data.stacks + 1;
            if ((int)Amount != correct)
                SetAmount(correct); // 触发第二次回调完成同步，第二次 Amount == correct 不再递归
            else
                InvokeDisplayAmountChanged();
        }
        return Task.CompletedTask;
    }

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
        var stacks = GetInternalData<Data>().stacks;

        if (stacks >= MaxSealStacks)
        {
            Flash();
            await element.OnEnhanced(choiceContext, this);
            var reduce = element.GetStacksToReduce(enhanced: true);
            if (reduce > 0)
                SetStacks(Math.Max(0, stacks - reduce));
        }
        else if (stacks >= ThresholdStacks)
        {
            Flash();
            await element.OnThreshold(choiceContext, this);
            var reduce = element.GetStacksToReduce(enhanced: false);
            if (reduce > 0)
                SetStacks(Math.Max(0, stacks - reduce));
        }
    }

    /// <summary>
    /// 切换当前印记属性。可在子类或外部卡牌中 await 调用，预留扩展空间（如未来的切换动画）。
    /// </summary>
    public virtual Task SetAttribute(PlayerChoiceContext ctx, SealAttribute attribute)
    {
        CurrentAttribute = attribute;
        return Task.CompletedTask;
    }
}
