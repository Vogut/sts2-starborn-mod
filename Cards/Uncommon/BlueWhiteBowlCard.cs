using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class BlueWhiteBowlCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        StarbornCardVars.Tuning(1, SealElementType.Wind),
        StarbornCardVars.Overload(2, SealElementType.Wind),
        StarbornCardVars.IfCanOverload(MarkSlot.Secondary),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(
            choiceContext, DynamicVars.Cards.IntValue, Owner);

        var canOverload = StarbornCmd.CanOverload(Owner, MarkSlot.Secondary);
        var elementType = canOverload
            ? ((SealElementVar)DynamicVars["Overload"]).ElementType
            : ((SealElementVar)DynamicVars["Tuning"]).ElementType;
        var consume = canOverload
            ? DynamicVars["Overload"].IntValue
            : DynamicVars["Tuning"].IntValue;

        if (canOverload)
        {
            await StarbornCmd.Overload(
                choiceContext, MarkSlot.Secondary, Owner,
                consume, elementType, this);
        }
        else
        {
            await StarbornCmd.Tuning(
                choiceContext, MarkSlot.Secondary, Owner,
                consume, elementType, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
