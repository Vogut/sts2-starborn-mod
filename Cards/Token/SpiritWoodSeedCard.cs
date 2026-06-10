using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Token;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class SpiritWoodSeedCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Token, TargetType.None
)
{
    private const int BaseConsume = 1;

    protected override bool IsPlayable =>
        StarbornCmd.CanTuning(Owner, MarkSlot.Primary);

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Tuning(BaseConsume, SealElementType.Wood),
        StarbornCardVars.Overload(BaseConsume + 1, SealElementType.Wood),
        StarbornCardVars.IfCanOverload()
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (StarbornCmd.CanOverload(Owner, MarkSlot.Primary))
            await StarbornCmd.Overload(choiceContext, MarkSlot.Primary, Owner, DynamicVars["Overload"].IntValue, SealElementType.Wood, this);
        else
            await StarbornCmd.Tuning(choiceContext, MarkSlot.Primary, Owner, DynamicVars["Tuning"].IntValue, SealElementType.Wood, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Ethereal);
    }
}
