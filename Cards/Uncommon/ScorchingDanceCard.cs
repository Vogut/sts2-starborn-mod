using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Cards.Uncommon;

/// <summary>
/// 灼热烈舞：1费稀有攻击。调谐2火（可超限3火）。对所有敌人造成11点伤害。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class ScorchingDanceCard() : StarbornCard(
    1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies
)
{
    protected override bool IsPlayable
    {
        get
        {
            var stacks = PrimaryStacks;
            var consume = stacks > ElementMarkState.ThresholdStacks
                ? DynamicVars["Overload"].IntValue
                : DynamicVars["Tuning"].IntValue;

            if (CombatState != null)
            {
                consume = stacks > ElementMarkState.ThresholdStacks
                    ? SealElementMarkHooks.ModifyOverloadConsume(CombatState, MarkSlot.Primary, consume)
                    : SealElementMarkHooks.ModifyTuningConsume(CombatState, MarkSlot.Primary, consume);
            }

            return consume >= 0 && stacks >= consume;
        }
    }

    private const int baseComuse = 2;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(11m, ValueProp.Move),
        StarbornCardVars.Tuning(baseComuse, SealElementType.Fire),
        StarbornCardVars.Overload(baseComuse + 1, SealElementType.Fire),
        StarbornCardVars.IfCanOverload(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState == null) return;

        var canOverload = PrimaryStacks > ElementMarkState.ThresholdStacks;
        if (canOverload)
        {
            var overloadElementType = ((SealElementVar)DynamicVars["Overload"]).ElementType;
            await StarbornCmd.Overload(choiceContext, MarkSlot.Primary, Owner,
                DynamicVars["Overload"].IntValue, overloadElementType, this);
        }
        else
        {
            var tuningElementType = ((SealElementVar)DynamicVars["Tuning"]).ElementType;
            await StarbornCmd.Tuning(choiceContext, MarkSlot.Primary, Owner,
                DynamicVars["Tuning"].IntValue, tuningElementType, this);
        }

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
