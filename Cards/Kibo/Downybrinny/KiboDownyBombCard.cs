using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.Downybrinny)]
public sealed class KiboDownyBombCard() : KiboCard(CardType.Attack, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.NormalKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(4, SealElementType.Light),
        StarbornCardVars.Overload(1, SealElementType.Light),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var elementType = ((SealElementVar)DynamicVars["ElementMark"]).ElementType;
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Primary, Owner,
            DynamicVars["ElementMark"].IntValue, elementType);
        await StarbornCmd.Overload(
            choiceContext, MarkSlot.Primary, Owner,
            DynamicVars["Overload"].IntValue, elementType, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ElementMark"].BaseValue += 1;
    }
}
