using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;

namespace STS2_Starborn.Element;

public sealed class LightElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Light;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_LIGHT.description");

    public override Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks)
        => Task.CompletedTask;

    public override Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks)
        => Task.CompletedTask;
}
