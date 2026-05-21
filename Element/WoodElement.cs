using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace STS2_Starborn.Element;

public sealed class WoodElement : Element
{
    public override SealElementType Attribute => SealElementType.Wood;

    public override LocString ElementDescription =>
        new LocString("powers", "STARBORN_ELEMENT_WOOD.description");

    public override Task OnThreshold(PlayerChoiceContext ctx, Player owner)
        => Task.CompletedTask;

    public override Task OnEnhanced(PlayerChoiceContext ctx, Player owner)
        => Task.CompletedTask;
}
