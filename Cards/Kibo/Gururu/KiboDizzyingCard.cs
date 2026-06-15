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
[KiboAbilityOf(KiboTypeId.Gururu, true)]
public sealed class KiboDizzyingCard() : KiboCard(1, CardType.Skill, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.UltimateKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(1),
        ElementMark(1, SealElementType.Any, "ElementMark", MarkSlot.Primary),
        ElementMark(1, SealElementType.Any, "ElementMarkSecondary", MarkSlot.Secondary),
        new CardsVar(2),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var energy = DynamicVars.Energy.IntValue;
        var markStacks = DynamicVars["ElementMark"].IntValue;
        var markStacksSecondary = DynamicVars["ElementMarkSecondary"].IntValue;
        var cards = DynamicVars.Cards.IntValue;

        // 随机选择：0 = 能量，1 = 印记，2 = 抽牌
        var choice = Owner.RunState.Rng.CombatCardSelection.NextInt(3);

        switch (choice)
        {
            case 0:
                // 获得能量
                await PlayerCmd.GainEnergy(energy, Owner);
                break;
            case 1:
                // 获得主/副印记各1层
                await SealElementMarkCmd.GainElementMarks(
                    choiceContext, MarkSlot.Primary, Owner, markStacks);
                await SealElementMarkCmd.GainElementMarks(
                    choiceContext, MarkSlot.Secondary, Owner, markStacksSecondary);
                break;
            case 2:
                // 抽牌
                await CardPileCmd.Draw(choiceContext, cards, Owner);
                break;
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1m);
        DynamicVars["ElementMark"].UpgradeValueBy(1m);
        DynamicVars["ElementMarkSecondary"].UpgradeValueBy(1m);
        DynamicVars.Cards.UpgradeValueBy(1m);
    }
}
