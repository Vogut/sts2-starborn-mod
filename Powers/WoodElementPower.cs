using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace STS2_Starborn.Powers;

/// <summary>
/// 木属性效果提供者。（施工中，效果待实现）
/// </summary>
public sealed class WoodElementPower : ElementPower
{

    public override SealElementType Attribute => SealElementType.Wood;

    public override LocString ElementDescription =>
        new LocString("powers", "STARBORN_ELEMENT_WOOD.description");

    public override Task OnThreshold(PlayerChoiceContext ctx, SealElementMarkPower source) =>
        // TODO: 实现木属性基础效果
        Task.CompletedTask;

    public override Task OnEnhanced(PlayerChoiceContext ctx, SealElementMarkPower source) =>
        // TODO: 实现木属性强化效果
        Task.CompletedTask;
}
