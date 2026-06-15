using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Rare;

/// <summary>
/// 首充双倍：失去金币，获得战斗结束时额外获得一次战斗奖励的能力。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class FirstTopUpBonusCard() : StarbornCard(
    3, CardType.Power, CardRarity.Rare, TargetType.Self
)
{
    protected override bool IsPlayable => Owner.Gold >= DynamicVars["Gold"].IntValue;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Gold", 12),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var goldCost = DynamicVars["Gold"].IntValue;
        await PlayerCmd.LoseGold(goldCost, Owner);

        await PowerCmd.Apply<FirstTopUpBonusPower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Gold"].UpgradeValueBy(-6);
    }
}
