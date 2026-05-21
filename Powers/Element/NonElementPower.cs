using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace STS2_Starborn.Powers;

/// <summary>
/// 无属性效果提供者。
/// 作为占位符使用，提供与其他属性相同的触发条件但无实际效果，确保在未选择属性时不消耗层数。
/// </summary>
public sealed class NonElementPower : ElementPower
{
    public override SealElementType Attribute => SealElementType.None;

    public override int TuningConsume => 0;
    public override int OverloadConsume => 0;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_NONE.description");

    public override Task OnThreshold(PlayerChoiceContext ctx, Player owner)
        => Task.CompletedTask;

    public override Task OnEnhanced(PlayerChoiceContext ctx, Player owner)
        => Task.CompletedTask;
}
