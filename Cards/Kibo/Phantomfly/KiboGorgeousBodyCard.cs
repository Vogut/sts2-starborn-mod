using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.Phantomfly)]
public sealed class KiboGorgeousBodyCard() : KiboCard(1, CardType.Skill, TargetType.AllEnemies)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.NormalKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Expose", 3),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var stacks = DynamicVars["Expose"].IntValue;
        var enemies = CombatState?.Enemies.Where(e => e.IsAlive).ToList();
        if (enemies == null || enemies.Count == 0)
            return;

        foreach (var enemy in enemies)
        {
            await PowerCmd.Apply<ExposePower>(
                choiceContext, enemy, stacks, Owner.Creature, this);
        }

        // Apply retain power to player — prevents Expose from being consumed on attack,
        // and removes all Expose at end of this turn
        await PowerCmd.Apply<GorgeousBodyPower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Expose"].UpgradeValueBy(4m);
    }
}
