using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_Starborn.Powers;

/// <summary>
/// 水属性效果提供者。（施工中，效果待实现）
/// </summary>
public sealed class WaterElementPower : ElementPower
{
    public override SealElementType Attribute => SealElementType.Water;

    public override LocString ElementDescription =>
        new LocString("powers", "STARBORN_ELEMENT_WATER.description");

    public override async Task OnThreshold(PlayerChoiceContext ctx, Player owner) =>
        await CreatureCmd.GainBlock(owner.Creature, 5m, ValueProp.Unpowered, null);

    public override async Task OnEnhanced(PlayerChoiceContext ctx, Player owner) =>
        await CreatureCmd.GainBlock(owner.Creature, 14m, ValueProp.Unpowered, null);
}
