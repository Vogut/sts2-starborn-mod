using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Powers;

/// <summary>
/// 随机上岗 Power：回合开始时，随机从 combat storage 中挑选一只奇波与 active 中的交换，
/// 然后自动释放该奇波的一张普通能力牌。
/// </summary>
[RegisterPower]
public class RandomDutyPower : StarbornPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        // 仅在玩家回合触发，且能力持有者必须在参与者中
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;

        // Owner 是 Creature，通过 combatState 找到对应的 Player
        var player = combatState.Players.FirstOrDefault(p => p.Creature == Owner);
        if (player == null) return;

        Flash();

        await KiboCmd.SummonRandom(new BlockingPlayerChoiceContext(), player);
        await KiboCmd.TryAutoPlayRandomNormalCard(player, combatState);
    }
}
