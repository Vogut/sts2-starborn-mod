using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

/// <summary>
/// 巨化打击：1费普通攻击。给目标施加1层变大，然后造成 基础伤害×当前变大层数 的伤害。
/// 升级：基础伤害 8→12。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class NeedMoreBiggerCard() : StarbornCard(
    1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 先给目标施加 1 层变大
        await PowerCmd.Apply<EnlargePower>(choiceContext,
            cardPlay.Target!, 1, Owner.Creature, this);

        // 造成 基础伤害 × 目标当前变大层数
        var stacks = cardPlay.Target!.GetPowerAmount<EnlargePower>();
        var damage = DynamicVars.Damage.BaseValue * stacks;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        // 基础伤害 8 → 12
        DynamicVars.Damage.UpgradeValueBy(4m);
    }
}
