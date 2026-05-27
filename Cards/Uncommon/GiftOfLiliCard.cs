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

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public class GiftOfLiliCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.None
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("A", 2),
        new IntVar("B", 1),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!IsUpgraded)
            await SealElementMarkCmd.SetElementType(choiceContext, MarkSlot.Primary, Owner, SealElementType.None);
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Primary, Owner, DynamicVars["A"].IntValue);

        await SealElementMarkCmd.SetElementType(choiceContext, MarkSlot.Secondary, Owner, SealElementType.None);
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Secondary, Owner, DynamicVars["B"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["B"].BaseValue += 1;
    }
}
