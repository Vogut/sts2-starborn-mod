using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.FlameCrystalArmor, true)]
public sealed class KiboDetonatingCrystalCard() : KiboCard(CardType.Skill, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.UltimateKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = base.CombatState;
        if (combatState == null) return;

        var enemies = IsUpgraded
            ? combatState.HittableEnemies.Where(e => e.IsAlive).ToList()
            : new List<Creature> { cardPlay.Target! };

        var burnAmount = Owner.Creature.Block;
        foreach (var enemy in enemies)
        {
            if (burnAmount > 0)
            {
                await PowerCmd.Apply<BurnPower>(choiceContext, enemy, burnAmount, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}


