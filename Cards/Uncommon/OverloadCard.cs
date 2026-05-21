using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public class OverloadCard() : StarbornCard(
    2, CardType.Skill, CardRarity.Uncommon, TargetType.None
)
{
    protected override bool IsPlayable => PrimaryStacks > 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Overload(10, SealElementType.Fire),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SealElementMarkCmd.SetElementType(choiceContext, MarkSlot.Primary, Owner, SealElementType.Fire);
        var need = ElementMarkManager.ThresholdStacks - PrimaryStacks;
        if (need > 0)
            await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Primary, Owner, need);
        await StarbornCmd.Overload(choiceContext, MarkSlot.Primary, Owner, 1, this);
        var remain = ElementMarkManager.MaxSealStacks - ElementMarkManager.GetStacks(Owner, MarkSlot.Primary);
        if (remain > 0)
            await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Primary, Owner, remain);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Overload"].BaseValue += 5;
    }
}
