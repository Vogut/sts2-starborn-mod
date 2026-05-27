using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class AlignmentCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.None
)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var primaryStacks = PrimaryStacks;
        var secondaryStacks = SecondaryStacks;

        if (primaryStacks > secondaryStacks)
        {
            await SealElementMarkCmd.GainElementMarks(
                choiceContext, MarkSlot.Secondary, Owner, primaryStacks - secondaryStacks);
        }
        else if (secondaryStacks > primaryStacks)
        {
            await SealElementMarkCmd.GainElementMarks(
                choiceContext, MarkSlot.Primary, Owner, secondaryStacks - primaryStacks);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
