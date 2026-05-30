using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public class StarbockCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.None
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(1, SealElementType.Any),
        new EnergyVar(3),
    ];

    protected override bool IsPlayable => PrimaryStacks >= 1 && SecondaryStacks >= 1;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var consume = DynamicVars["ElementMark"].IntValue;
        await SealElementMarkCmd.RemoveElementMarks(choiceContext, MarkSlot.Primary, Owner, consume);
        await SealElementMarkCmd.RemoveElementMarks(choiceContext, MarkSlot.Secondary, Owner, consume);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
