using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Powers;

/// <summary>
/// 顺风：回合开始时调谐指定元素。
/// 元素类型和消耗量由 TailwindCard 在 OnPlay 中设置。
/// </summary>
[RegisterPower]
public class TailwindPower : StarbornPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public SealElementType TuningElementType { get; set; } = SealElementType.Wind;
    public int TuningConsume { get; set; } = 0;

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;

        Flash();

        if (Owner.Player == null) return;

        await StarbornCmd.Tuning(
            new ThrowingPlayerChoiceContext(),
            MarkSlot.Primary, Owner.Player,
            TuningConsume, TuningElementType, null);
    }
}
