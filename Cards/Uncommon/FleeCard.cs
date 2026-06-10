using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class FleeCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Uncommon, TargetType.Self
)
{
    private const int BlockPerMark = 5;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("BlockPerMark", BlockPerMark)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var primaryStacks = ElementMarkState.GetStacks(Owner, MarkSlot.Primary);
        var secondaryStacks = ElementMarkState.GetStacks(Owner, MarkSlot.Secondary);
        var totalStacks = primaryStacks + secondaryStacks;

        if (totalStacks > 0)
        {
            var blockAmount = totalStacks * DynamicVars["BlockPerMark"].IntValue;
            await CreatureCmd.GainBlock(Owner.Creature, blockAmount, ValueProp.Move, cardPlay);

            await SealElementMarkCmd.RemoveElementMarks(choiceContext, MarkSlot.Primary, Owner, primaryStacks);
            await SealElementMarkCmd.RemoveElementMarks(choiceContext, MarkSlot.Secondary, Owner, secondaryStacks);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BlockPerMark"].BaseValue += 1;
    }
}
