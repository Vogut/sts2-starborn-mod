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
    /// <summary>此属性对应的 <see cref="SealAttribute"/> 枚举值</summary>
    public abstract SealAttribute Attribute { get; }

    /// <summary>
    /// 属性激活时在印记 Power 上展示的描述文本（本地化字符串）。
    /// </summary>
    public abstract LocString ElementDescription { get; }

    /// <summary>基础效果：印记达到 <see cref="SealMarkPower.ThresholdStacks"/> 层时触发</summary>
    public abstract Task OnThreshold(PlayerChoiceContext ctx, SealMarkPower source);

    /// <summary>强化效果：印记达到 <see cref="SealMarkPower.MaxSealStacks"/> 层时触发</summary>
    public abstract Task OnEnhanced(PlayerChoiceContext ctx, SealMarkPower source);

    /// <summary>
    /// 根据 <see cref="SealAttribute"/> 返回对应的 <see cref="ElementPower"/> 单例实例；
    /// <see cref="SealAttribute.None"/> 或未知属性返回 <c>null</c>。
    /// </summary>
    public static ElementPower? For(SealAttribute attribute) => attribute switch
    {
        SealAttribute.Fire  => ModelDb.Power<FireElementPower>(),
        SealAttribute.Water => ModelDb.Power<WaterElementPower>(),
        SealAttribute.Wood  => ModelDb.Power<WoodElementPower>(),
        _                   => null,
    };
}
