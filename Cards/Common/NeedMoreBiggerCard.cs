using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class NeedMoreBiggerCard() : StarbornCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CalculationBaseVar(0m),
        new ExtraDamageVar(8m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature? target) =>
            target?.GetPowerAmount<EnlargePower>() ?? 1),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await PowerCmd.Apply<EnlargePower>(choiceContext,
            cardPlay.Target, 1, Owner.Creature, this);
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(4m);
    }
}
