using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;

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

        var combatStorage = KiboPileManager.GetStorageCombatPile(player);
        if (combatStorage == null) return;

        // 收集 combatStorage 中所有可用奇波类型（排除 RepCard，排除当前活跃类型）
        var candidates = KiboPileManager.GetKiboTypesInPile(combatStorage).ToList();
        if (candidates.Count == 0) return;

        var activeType = KiboPileManager.GetActiveKiboType(player);
        if (activeType != null) candidates.Remove(activeType.Value);
        if (candidates.Count == 0) return;

        // 随机选一个与当前活跃不同的奇波类型
        var targetType = candidates[Random.Shared.Next(candidates.Count)];

        // 播放闪光动效，提示玩家能力触发
        Flash();

        // 切换到随机选中的奇波类型
        await KiboPileManager.ActivateType(player, targetType);

        // 从新活跃的牌堆中随机选一张普通能力牌自动打出
        await KiboPileManager.TryAutoPlayRandomNormalCard(player, combatState);
    }
}
