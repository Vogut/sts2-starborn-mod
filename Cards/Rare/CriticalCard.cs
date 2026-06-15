using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Rare;

/// <summary>
/// 临界：1费稀有技能。消耗。获得5/5当前属性印记。本回合印记不会减少并在回合结束时清空所有印记。
/// 升级：耗能-1。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class CriticalCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Rare, TargetType.Self
)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ElementMark(5, SealElementType.Any, "PrimaryMark"),
        ElementMark(5, SealElementType.Any, "SecondaryMark", MarkSlot.Secondary),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<CriticalPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var primaryStacks = DynamicVars["PrimaryMark"].IntValue;
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Primary, Owner, primaryStacks);

        var secondaryStacks = DynamicVars["SecondaryMark"].IntValue;
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Secondary, Owner, secondaryStacks);

        await PowerCmd.Apply<CriticalPower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
