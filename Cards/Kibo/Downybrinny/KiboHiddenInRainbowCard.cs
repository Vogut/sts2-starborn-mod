using MegaCrit.Sts2.Core.Commands;
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
[KiboAbilityOf(KiboTypeId.Downybrinny, true)]
public sealed class KiboHiddenInRainbowCard() : KiboCard(CardType.Skill, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.UltimateKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ElementMark(1, SealElementType.Any, "ElementMark", MarkSlot.Primary),
        ElementMark(1, SealElementType.Any, "ElementMarkSecondary", MarkSlot.Secondary),
        new EnergyVar(2),
        new CardsVar(2),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var stacks = DynamicVars["ElementMark"].IntValue;
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Primary, Owner, stacks);
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Secondary, Owner, stacks);
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await CardPileCmd.Draw(
            choiceContext, DynamicVars.Cards.IntValue, Owner);
        await KiboCmd.SummonRandom(choiceContext, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
        DynamicVars.Cards.BaseValue += 1;
    }
}
