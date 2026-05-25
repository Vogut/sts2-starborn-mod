using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.ValueProps;

namespace STS2_Starborn.Element;

public sealed class WaterElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Water;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_WATER.description");

    public override async Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks) =>
        await CreatureCmd.GainBlock(owner.Creature, 5m, ValueProp.Unpowered, null);

    public override async Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks) =>
        await CreatureCmd.GainBlock(owner.Creature, 14m, ValueProp.Unpowered, null);
}
