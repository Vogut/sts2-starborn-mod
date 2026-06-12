using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Cards.Token;
using STS2_Starborn.Commands;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;
using STS2_Starborn.UI;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class ForestWishCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(2, SealElementType.Wood)
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        new CompactCardGridHoverTip(
            [ModelDb.GetById<CardModel>(ModelDb.GetId(typeof(SpiritWoodSeedCard)))])
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Primary, Owner, DynamicVars["ElementMark"].IntValue);

        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(typeof(SpiritWoodSeedCard)));
        var card = combatState.CreateCard(canonical, Owner);
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ElementMark"].BaseValue += 1;
    }
}
