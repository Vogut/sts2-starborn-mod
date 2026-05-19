using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2_Starborn.Cards;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

/// <summary>
/// 调谐卡：切换为火属性、叠加层数，调谐后造成当前印记层数的伤害。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public class TuningCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.None
)
{
    protected override bool IsPlayable =>
        StarbornCmd.CanTuning(PrimaryMark, DynamicVars["Tuning"].IntValue);

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(2, SealElementType.Fire),
        StarbornCardVars.Tuning(2, SealElementType.Fire),
        new CalculationBaseVar(0),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Unpowered).WithMultiplier((card, _) =>
            card.Owner.Creature.FindPower<PrimaryMarkPower>()?.DisplayAmount ?? 0),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await StarbornCmd.Tuning(choiceContext, PrimaryMark!, DynamicVars["Tuning"].IntValue, SealElementType.Fire, this);
        await SealElementMarkCmd.GainElementMarks(choiceContext, PrimaryMark!, DynamicVars["ElementMark"].IntValue, this);
        var damage = DynamicVars["CalculatedDamage"].BaseValue;
        if (damage > 0)
            await CreatureCmd.Damage(choiceContext, PrimaryMark!.CombatState.HittableEnemies, damage, ValueProp.Unpowered, PrimaryMark.Owner, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ElementMark"].BaseValue += 1; // 2 → 3
    }
}
