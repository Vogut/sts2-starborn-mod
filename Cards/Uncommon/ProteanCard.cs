using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

/// <summary>
/// 变换自在：1费罕见能力。每当你切换属性时，获得3点格挡。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class ProteanCard() : StarbornCard(
    1, CardType.Power, CardRarity.Uncommon, TargetType.Self
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Block", 3),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var blockAmount = (int)DynamicVars["Block"].BaseValue;

        // 施加变换自在 Power
        await PowerCmd.Apply<ProteanPower>(choiceContext,
            Owner.Creature, blockAmount, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Block"].UpgradeValueBy(1m);
    }
}
