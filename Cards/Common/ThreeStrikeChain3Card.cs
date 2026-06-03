using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class ThreeStrikeChain3Card() : StarbornCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy,
    shouldShowInCardLibrary: false
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(12, ValueProp.Move)
    ];

    public override bool HasTurnEndInHandEffect => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(typeof(ThreeStrikeChain1Card)));
        var phase1 = combatState.CreateCard(canonical, Owner);
        if (IsUpgraded)
            CardCmd.Upgrade(phase1);
        await CardPileCmd.AddGeneratedCardToCombat(phase1, PileType.Hand, Owner);
    }

    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(typeof(ThreeStrikeChain1Card)));
        var phase1 = combatState.CreateCard(canonical, Owner);
        if (IsUpgraded)
            CardCmd.Upgrade(phase1);
        await CardPileCmd.AddGeneratedCardToCombat(phase1, PileType.Hand, Owner);
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        return PileType.Exhaust;
    }

    protected override PileType GetResultPileTypeForOnTurnEndInHandEffect()
    {
        return PileType.Exhaust;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
