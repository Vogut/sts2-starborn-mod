using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Ancient;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class KiboComboCard() : StarbornCard(
    2, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        new DynamicVar("Vulnerable", 3m)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");

        await PowerCmd.Apply<VulnerablePower>(
            choiceContext, cardPlay.Target,
            DynamicVars["Vulnerable"].IntValue,
            Owner.Creature, this);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        var combatState = Owner.Creature.CombatState!;
        var pile = KiboPileManager.GetActivePile(Owner);
        if (pile == null) return;

        var normalCards = pile.Cards.Where(c => c.HasModKeyword(KiboKeywords.NormalKeyword)).ToList();
        foreach (var card in normalCards)
            await KiboCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, combatState);

        var ultimateCards = pile.Cards.Where(c => c.HasModKeyword(KiboKeywords.UltimateKeyword)).ToList();
        foreach (var card in ultimateCards)
            await KiboCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, combatState);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
