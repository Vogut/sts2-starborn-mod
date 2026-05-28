using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class LanceHurlCard() : StarbornCard(
    3, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(19m, ValueProp.Move),
    ];

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        await base.AfterCardEnteredCombat(card);
        if (card == this)
        {
            ElementMarkManager.MarksChanged += UpdateCost;
            UpdateCost();
        }
    }

    public override async Task BeforeCardRemoved(CardModel card)
    {
        if (card == this)
            ElementMarkManager.MarksChanged -= UpdateCost;
        await base.BeforeCardRemoved(card);
    }

    private void UpdateCost()
    {
        EnergyCost.EndOfTurnCleanup();
        if (PrimaryStacks >= 3 && SecondaryStacks >= 3)
            EnergyCost.SetThisTurn(0);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
