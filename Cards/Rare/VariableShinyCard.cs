using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Character;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class VariableShinyCard : StarbornCard
{
    public VariableShinyCard() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatStorage = KiboPileManager.GetStorageCombatPile(Owner);
        var activePile = KiboPileManager.GetActivePile(Owner);

        foreach (var pile in new[] { combatStorage, activePile })
        {
            if (pile == null) continue;
            foreach (var card in pile.Cards.ToList())
            {
                if (card is KiboCard && card.IsUpgradable)
                    CardCmd.Upgrade(card);
            }
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
