using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace STS2_Starborn.Powers;

/// <summary>
/// 水属性效果提供者。（施工中，效果待实现）
/// </summary>
public sealed class WaterElementPower : ElementPower
{

    public override SealAttribute Attribute => SealAttribute.Water;

    public override LocString ElementDescription =>
        new LocString("powers", "STARBORN_ELEMENT_WATER.description");

    public override Task OnThreshold(PlayerChoiceContext ctx, SealMarkPower source) =>
        // TODO: 实现水属性基础效果
        Task.CompletedTask;

    public override Task OnEnhanced(PlayerChoiceContext ctx, SealMarkPower source) =>
        // TODO: 实现水属性强化效果
        Task.CompletedTask;
}
