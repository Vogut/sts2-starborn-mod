using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class PayToWinCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Rare, TargetType.None
)
{
    protected override bool IsPlayable => Owner.Gold >= 6;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Empower", 1),
        new IntVar("Gold", 6),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<TuningEmpowermentPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var goldCost = DynamicVars["Gold"].IntValue;
        await PlayerCmd.LoseGold(goldCost, Owner);

        var empowerStacks = DynamicVars["Empower"].IntValue;
        await PowerCmd.Apply<TuningEmpowermentPower>(
            choiceContext, Owner.Creature, empowerStacks, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Empower"].UpgradeValueBy(1);
    }
}
