using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Rare;

/// <summary>
/// 律动：2费稀有能力。每次进行调谐/超限后，对随机一名敌人造成3点伤害。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class RhythmCard() : StarbornCard(
    2, CardType.Power, CardRarity.Rare, TargetType.Self
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<RhythmPower>(3m),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<RhythmPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RhythmPower>(
            choiceContext, Owner.Creature, DynamicVars["RhythmPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RhythmPower"].UpgradeValueBy(1m);
    }
}
