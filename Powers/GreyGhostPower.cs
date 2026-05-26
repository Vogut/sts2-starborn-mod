using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

/// <summary>
/// 灰色幽灵（Phase 2）：攻击伤害×2，受到攻击伤害变为1点。敌方回合结束时移除。
/// 由 GreyGhostPrepPower 在回合开始时施加。
/// </summary>
[RegisterPower]
public class GreyGhostPower : StarbornPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer != Owner && !Owner.Pets.Contains(dealer))
            return 1m;
        if (!props.IsPoweredAttack())
            return 1m;
        if (cardSource == null)
            return 1m;
        return 2m;
    }

    public override decimal ModifyHpLostAfterOsty(
        Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!CombatManager.Instance.IsInProgress)
            return amount;
        if (target != Owner)
            return amount;
        if (amount < 1m)
            return amount;
        return 1m;
    }

    public override Task AfterModifyingHpLostAfterOsty()
    {
        Flash();
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageCap(
        Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
            return decimal.MaxValue;
        return 1m;
    }

    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy && participants.Contains(Owner))
            await PowerCmd.Decrement(this);
    }
}
