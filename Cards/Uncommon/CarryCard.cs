using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

/// <summary>
/// 带躺：1费罕见能力。获得1点力量。获得5层奇波强化。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class CarryCard() : StarbornCard(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Strength", 1),
        new IntVar("Empower", 5),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var strengthStacks = (int)DynamicVars["Strength"].BaseValue;
        var empowerStacks = (int)DynamicVars["Empower"].BaseValue;

        // 获得力量
        await PowerCmd.Apply<StrengthPower>(choiceContext,
            Owner.Creature, strengthStacks, Owner.Creature, this);

        // 获得奇波强化
        await PowerCmd.Apply<KiboEmpowermentPower>(choiceContext,
            Owner.Creature, empowerStacks, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Strength"].UpgradeValueBy(1m);
        DynamicVars["Empower"].UpgradeValueBy(5m);
    }
}
