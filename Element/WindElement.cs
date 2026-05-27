using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace STS2_Starborn.Element;

public sealed class WindElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Wind;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_WIND.description");

    public override Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks)
        => Task.CompletedTask;

    public override Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks)
        => Task.CompletedTask;
}
