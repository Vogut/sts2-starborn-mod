using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.HealthBars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

/// <summary>
/// 灼烧 Debuff：可堆叠。在目标回合开始时造成等同于层数的不可格挡伤害，随后层数减半。
/// 参考原版 PoisonPower 的触发时机和伤害结算方式。
/// </summary>
[RegisterPower]
public class BurnPower : StarbornPower, IHealthBarForecastSource
{
    private static readonly Color ForecastColor = new("#FF4500");

    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public int CalculateTotalDamageNextTurn()
    {
        return Amount;
    }

    public IEnumerable<HealthBarForecastSegment> GetHealthBarForecastSegments(HealthBarForecastContext context)
    {
        return HealthBarForecasts
            .FromRight(context, ForecastColor)
            .AtSideTurnStart(context.Creature.Side, CalculateTotalDamageNextTurn())
            .Build();
    }

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner))
            return;

        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(), Owner, Amount,
            ValueProp.Unblockable | ValueProp.Unpowered, null, null);

        if (!Owner.IsAlive)
            return;

        var newAmount = (int)Amount / 2;
        var offset = newAmount - (int)Amount;
        await PowerCmd.ModifyAmount(
            new ThrowingPlayerChoiceContext(), this, offset, null, null);
    }
}
