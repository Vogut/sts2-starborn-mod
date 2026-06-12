using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class OutOfControlFlightCard() : StarbornCard(
    2, CardType.Attack, CardRarity.Uncommon, TargetType.RandomEnemy
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move)
    ];

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        await base.AfterCardEnteredCombat(card);
        if (card == this)
        {
            ElementMarkState.MarksChanged += UpdateCost;
            UpdateCost();
        }
    }

    public override async Task BeforeCardRemoved(CardModel card)
    {
        if (card == this)
            ElementMarkState.MarksChanged -= UpdateCost;
        await base.BeforeCardRemoved(card);
    }

    private void UpdateCost()
    {
        if (PrimaryElementType != SealElementType.None)
            EnergyCost.SetThisCombat(0);
        else
            EnergyCost.SetThisCombat(2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null) return;

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingRandomOpponents(CombatState)
            .Execute(choiceContext);

        await SealElementMarkCmd.SetElementType(choiceContext, MarkSlot.Primary, Owner, SealElementType.None);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}
