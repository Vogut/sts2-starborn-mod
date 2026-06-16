using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

/// <summary>
/// 旋光：1费罕见技能。调谐1光（可超限）。使所有敌人的暴晒层数翻倍。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class SpinningLightCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.Self
)
{
    protected override bool IsPlayable => StarbornCmd.CanTuning(Owner, MarkSlot.Primary);

    private const int baseConsume = 1;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        Tuning(baseConsume, SealElementType.Light, "Tuning", MarkSlot.Primary),
        Overload(baseConsume + 1, SealElementType.Light, "Overload", MarkSlot.Primary),
        StarbornCardVars.IfCanOverload(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null) return;

        // 调谐/超限光属性
        var canOverload = StarbornCmd.CanOverload(Owner, MarkSlot.Primary);
        var elementType = canOverload
            ? ((SealElementVar)DynamicVars["Overload"]).ElementType
            : ((SealElementVar)DynamicVars["Tuning"]).ElementType;
        var consume = canOverload
            ? DynamicVars["Overload"].IntValue
            : DynamicVars["Tuning"].IntValue;

        if (canOverload)
        {
            await StarbornCmd.Overload(choiceContext, MarkSlot.Primary, Owner,
                consume, elementType, this);
        }
        else
        {
            await StarbornCmd.Tuning(choiceContext, MarkSlot.Primary, Owner,
                consume, elementType, this);
        }

        // 使所有敌人的暴晒层数翻倍
        var enemies = CombatState.Enemies.Where(e => e.IsAlive).ToList();
        foreach (var enemy in enemies)
        {
            var exposePower = enemy.GetPower<ExposePower>();
            if (exposePower != null)
            {
                var currentStacks = exposePower.Amount;
                // 翻倍：再施加相同层数的暴晒
                await PowerCmd.Apply<ExposePower>(
                    choiceContext, enemy, currentStacks, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Tuning"].UpgradeValueBy(-1m);
        DynamicVars["Overload"].UpgradeValueBy(-1m);
    }
}
