using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public class GiftOfLiliCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.None
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(2, SealElementType.None, name: "Primary"),
        StarbornCardVars.ElementMark(2, SealElementType.None, name: "Secondary"),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        await SealElementMarkCmd.SetElementType(choiceContext, MarkSlot.Primary, Owner, SealElementType.None);
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Primary, Owner, DynamicVars["Primary"].IntValue);

        await SealElementMarkCmd.SetElementType(choiceContext, MarkSlot.Secondary, Owner, SealElementType.None);
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Secondary, Owner, DynamicVars["Secondary"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Primary"].BaseValue += 1;
    }
}
