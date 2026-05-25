using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace STS2_Starborn.Element;

public sealed class WoodElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Wood;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_WOOD.description");

    public override Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks)
        => Task.CompletedTask;

    public override Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks)
        => Task.CompletedTask;
}
