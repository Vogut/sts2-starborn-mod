using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Rare;

/// <summary>
/// 奥传·月轮：0费攻击牌。失去你的所有印记。每失去一枚印记，对所有敌人造成11点伤害（升级后13点）。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public class ArcaneMoonwheelCard() : StarbornCard(
    0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(11m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var totalStacks = PrimaryStacks + SecondaryStacks;

        await SealElementMarkCmd.RemoveElementMarks(choiceContext, MarkSlot.Primary, Owner, PrimaryStacks);
        await SealElementMarkCmd.RemoveElementMarks(choiceContext, MarkSlot.Secondary, Owner, SecondaryStacks);

        if (totalStacks > 0)
        {
            var damagePerStack = DynamicVars.Damage.BaseValue;

            for (int i = 0; i < totalStacks; i++)
            {
                await DamageCmd.Attack(damagePerStack)
                    .FromCard(this)
                    .TargetingAllOpponents(CombatState!)
                    .Execute(choiceContext);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
