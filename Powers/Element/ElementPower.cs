using MegaCrit.Sts2.Core.Entities.Players;
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

    public virtual int TuningConsume => 1;
    public virtual int OverloadConsume => 2;

    public abstract Task OnThreshold(PlayerChoiceContext ctx, Player owner);
    public abstract Task OnEnhanced(PlayerChoiceContext ctx, Player owner);

    public static ElementPower For(SealElementType attribute) => attribute switch
    {
        SealElementType.Fire  => ModelDb.Power<FireElementPower>(),
        SealElementType.Water => ModelDb.Power<WaterElementPower>(),
        SealElementType.Wood  => ModelDb.Power<WoodElementPower>(),
        _                     => ModelDb.Power<NonElementPower>(),
    };
}
