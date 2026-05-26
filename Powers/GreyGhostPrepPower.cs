using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

/// <summary>
/// 航空准备（Phase 1）：下回合开始时获得灰色幽灵，随后移除此能力。
/// 参照 ShadowStepPower 的两阶段设计。
/// </summary>
[RegisterPower]
public class GreyGhostPrepPower : StarbornPower
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
        if (!participants.Contains(Owner)) return;

        await PowerCmd.Apply<GreyGhostPower>(
            new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
        await PowerCmd.Remove(this);
    }
}
