using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

/// <summary>
/// 随机上岗：2费罕见能力牌。
/// 给予玩家"随机上岗"Power：每回合开始时随机从后备奇波中换岗并释放技能。
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class RandomDutyCard() : StarbornCard(
    2, CardType.Power, CardRarity.Uncommon, TargetType.None
)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 对卡牌持有者施加随机上岗 Power（1层，不可叠加为多层，仅作为开关存在）
        await PowerCmd.Apply<RandomDutyPower>(choiceContext,
            Owner.Creature, 1, Owner.Creature, this);
    }
}
