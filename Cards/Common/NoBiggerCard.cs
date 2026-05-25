using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

/// <summary>
/// 不够小！：1费普通技能。获得7格挡，给目标施加1层变小和1层虚弱。
/// 升级：施加2层虚弱。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class NoBiggerCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy
)
{
    public override bool GainsBlock => true;

    private int WeakStacks => IsUpgraded ? 2 : 1;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 格挡
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        // 给目标施加 1 层变小
        await PowerCmd.Apply<ShrinkBuffPower>(choiceContext,
            cardPlay.Target!, 1, Owner.Creature, this);

        // 给目标施加虚弱（升级前1层，升级后2层）
        await PowerCmd.Apply<WeakPower>(choiceContext,
            cardPlay.Target!, WeakStacks, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        // 升级仅改变虚弱层数（WeakStacks 属性已处理）
    }
}
