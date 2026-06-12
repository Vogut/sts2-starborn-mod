using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Token;

public abstract class MarkSelectCard(SealElementType elementType)
    : StarbornCard(0, CardType.Skill, CardRarity.Token, TargetType.Self, false), IElementChoosable
{
    public override bool CanBeGeneratedInCombat => false;
    public SealElementType ElementType => elementType;

    public async Task OnChosen(PlayerChoiceContext ctx, Player player, MarkSlot slot)
    {
        await SealElementMarkCmd.SetElementType(ctx, slot, player, ElementType);
    }
}
