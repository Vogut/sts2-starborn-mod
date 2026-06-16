using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Uncommon;

/// <summary>
/// 挥舞巨斧：2费罕见攻击。对所有敌人造成15点伤害。调谐1主属性2次（可超限）。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class WieldAxeCard() : StarbornCard(
    2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies
)
{
    protected override bool IsPlayable => StarbornCmd.CanTuning(Owner, MarkSlot.Primary);

    private const int baseConsume = 1;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(15m, ValueProp.Move),
        Tuning(baseConsume, SealElementType.Any, "Tuning", MarkSlot.Primary),
        Overload(baseConsume, SealElementType.Any, "Overload", MarkSlot.Primary),
        StarbornCardVars.IfCanOverload(),
        new IntVar("Repeat", 2),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null) return;

        // 对所有敌人造成伤害
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);

        // 调谐/超限主属性2次
        var canOverload = StarbornCmd.CanOverload(Owner, MarkSlot.Primary);
        var elementType = canOverload
            ? ((SealElementVar)DynamicVars["Overload"]).ElementType
            : ((SealElementVar)DynamicVars["Tuning"]).ElementType;
        var consume = canOverload
            ? DynamicVars["Overload"].IntValue
            : DynamicVars["Tuning"].IntValue;
        var repeatTimes = (int)DynamicVars["Repeat"].BaseValue;

        for (int i = 0; i < repeatTimes; i++)
        {
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
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
