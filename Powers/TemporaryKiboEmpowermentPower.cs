using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

[RegisterPower]
public class TemporaryKiboEmpowermentPower : StarbornPower, ITemporaryPower
{
    private bool _shouldIgnoreNextInstance;

    public AbstractModel OriginModel => this;
    public PowerModel InternallyAppliedPower => ModelDb.Power<KiboEmpowermentPower>();
    public void IgnoreNextInstance() => _shouldIgnoreNextInstance = true;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override async Task BeforeApplied(
        Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
            return;
        }
        await PowerCmd.Apply<KiboEmpowermentPower>(
            new ThrowingPlayerChoiceContext(), target, amount, applier, cardSource, silent: true);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount,
        Creature? applier, CardModel? cardSource)
    {
        if (amount == Amount || power != this)
            return;
        if (_shouldIgnoreNextInstance)
        {
            _shouldIgnoreNextInstance = false;
            return;
        }
        await PowerCmd.Apply<KiboEmpowermentPower>(
            choiceContext, Owner, amount, applier, cardSource, silent: true);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
            return;
        Flash();
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<KiboEmpowermentPower>(
            choiceContext, Owner, -Amount, Owner, null, silent: true);
    }
}
