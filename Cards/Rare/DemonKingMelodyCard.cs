using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class DemonKingMelodyCard() : StarbornCard(
    2, CardType.Skill, CardRarity.Rare, TargetType.None
)
{
    protected override bool IsPlayable =>
        ElementMarkManager.GetSwitchedTypeCount(Owner) >= 5;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var enemies = Owner.Creature.CombatState!.Enemies.ToList();
        await CreatureCmd.Kill(enemies);
    }
}
