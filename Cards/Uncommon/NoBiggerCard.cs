using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class NoBiggerCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy
)
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7m, ValueProp.Move),
        new DynamicVar("Weak", 1m),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await PowerCmd.Apply<ShrinkBuffPower>(choiceContext,
            cardPlay.Target, 1, Owner.Creature, this);
        await PowerCmd.Apply<WeakPower>(choiceContext,
            cardPlay.Target, DynamicVars["Weak"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Weak"].UpgradeValueBy(1m);
    }
}
