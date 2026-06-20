using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class AdaptabilityCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        ElementMark(1, SealElementType.Any, "Primary", MarkSlot.Primary),
        ElementMark(1, SealElementType.Any, "Secondary", MarkSlot.Secondary),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var drawn = await CardPileCmd.Draw(choiceContext, DynamicVars["Cards"].IntValue, Owner);

        foreach (var card in drawn)
        {
            if (card.Type == CardType.Attack)
            {
                await SealElementMarkCmd.GainElementMarks(
                    choiceContext, MarkSlot.Primary, Owner, DynamicVars["Primary"].IntValue);
            }

            if (card.Type == CardType.Skill)
            {
                await SealElementMarkCmd.GainElementMarks(
                    choiceContext, MarkSlot.Secondary, Owner, DynamicVars["Secondary"].IntValue);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1);
    }
}
