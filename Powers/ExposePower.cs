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

[RegisterPower]
public class ExposePower : StarbornPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, decimal amount,
        ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner)
            return;
        if (!props.HasFlag(ValueProp.Move))
            return;

        Flash();
        var stacks = Amount;

        // With GorgeousBody: Expose triggers but is NOT consumed on attack
        var hasRetain = Owner.CombatState?.Players
            .Any(p => p.Creature.GetPower<GorgeousBodyPower>() != null) ?? false;

        if (!hasRetain)
            await PowerCmd.Remove(this);

        await CreatureCmd.Damage(choiceContext, target, stacks,
            ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner, null);
    }
}
