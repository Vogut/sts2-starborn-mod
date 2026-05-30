using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.MelodiousVine)]
public sealed class KiboSeedStormCard() : KiboCard(CardType.Attack, TargetType.None)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.NormalKeyword,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(6, ValueProp.Move),
        StarbornCardVars.ElementMark(1, SealElementType.Wood),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var dmg = DynamicVars.Damage.BaseValue;
        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature.CombatState!.HittableEnemies,
            dmg,
            ValueProp.Unpowered,
            Owner.Creature,
            this);

        var marksPerEnemy = DynamicVars["ElementMark"].IntValue;
        var enemyCount = Owner.Creature.CombatState!.Enemies.Count();
        var totalMarks = enemyCount * marksPerEnemy;
        var elementType = ((SealElementVar)DynamicVars["ElementMark"]).ElementType;
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Primary, Owner, totalMarks, elementType);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}
