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
    2, CardType.Skill, CardRarity.Uncommon, TargetType.Self
)
{
    protected override bool IsPlayable => PrimaryStacks > 0;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Overload(1, SealElementType.Fire),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await StarbornCmd.Overload(choiceContext, MarkSlot.Primary, Owner, DynamicVars["Overload"].IntValue, SealElementType.Fire, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Overload"].BaseValue += 5;
    }
}
