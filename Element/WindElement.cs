using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace STS2_Starborn.Element;

public sealed class WindElement : StarbornElement
{
    public override SealElementType Attribute => SealElementType.Wind;

    public override LocString ElementDescription =>
        new LocString("powers", "STS2_STARBORN_ELEMENT_WIND.description");

    public override async Task OnThreshold(PlayerChoiceContext ctx, Player owner, int stacks, CardModel? source = null, IReadOnlyList<Creature>? targets = null) =>
        await CardPileCmd.Draw(ctx, 1, owner);

    public override async Task OnEnhanced(PlayerChoiceContext ctx, Player owner, int stacks, CardModel? source = null, IReadOnlyList<Creature>? targets = null)
    {
        await CardPileCmd.Draw(ctx, 1, owner);
        await PlayerCmd.GainEnergy(1, owner);
    }
}
