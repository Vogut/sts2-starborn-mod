using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
namespace STS2_Starborn.Powers;

/// <summary>
/// 属性效果提供者的抽象基类，作为纯行为委托对象使用，不继承 PowerModel，
/// 不通过 RegisterPower 注册为游戏内 Power。
/// 每种属性子类定义自己的本地化描述及触发效果。
/// </summary>
public abstract class ElementPower : StarbornPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
    /// <summary>此属性对应的 <see cref="SealElementType"/> 枚举值</summary>
    public abstract SealElementType Attribute { get; }

    /// <summary>
    /// 属性激活时在印记 Power 上展示的描述文本（本地化字符串）。
    /// </summary>
    public abstract LocString ElementDescription { get; }

    /// <summary>基础效果：印记达到 <see cref="SealElementMarkPower.ThresholdStacks"/> 层时触发</summary>
    public abstract Task OnThreshold(PlayerChoiceContext ctx, SealElementMarkPower source);

    /// <summary>强化效果：印记达到 <see cref="SealElementMarkPower.MaxSealStacks"/> 层时触发</summary>
    public abstract Task OnEnhanced(PlayerChoiceContext ctx, SealElementMarkPower source);

    /// <summary>
    /// 触发后消耗的印记层数。默认基础触发消耗 1 层，强化触发消耗 2 层。
    /// 返回 0 表示不消耗层数。各属性可按需重写。
    /// </summary>
    public virtual int GetStacksToReduce(bool enhanced) => enhanced ? 2 : 1;

    /// <summary>
    /// 根据 <see cref="SealElementType"/> 返回对应的 <see cref="ElementPower"/> 规范实例（由框架 ModelDb 管理）。
    /// 始终返回非 null；<see cref="SealElementType.None"/> 返回 <see cref="NonElementPower"/>。
    /// </summary>
    public static ElementPower For(SealElementType attribute) => attribute switch
    {
        SealElementType.Fire  => ModelDb.Power<FireElementPower>(),
        SealElementType.Water => ModelDb.Power<WaterElementPower>(),
        SealElementType.Wood  => ModelDb.Power<WoodElementPower>(),
        _                   => ModelDb.Power<NonElementPower>(),
    };
}
