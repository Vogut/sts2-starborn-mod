using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

/// <summary>
/// "你已疾苦" 效果：下回合开始时多抽牌。
/// 模仿 DrawCardsNextTurnPower 的实现。
/// </summary>
[RegisterPower]
public class YouAreSufferingPower : StarbornPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != Owner.Player)
            return count;

        if (AmountOnTurnStart == 0)
            return count;

        return count + Amount;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (participants.Contains(Owner) && AmountOnTurnStart != 0)
        {
            await PowerCmd.Remove(this);
        }
    }
}
