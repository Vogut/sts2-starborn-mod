using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_Starborn.Powers;

[RegisterPower]
public class SurgePower : TemporaryKiboEmpowermentPower
{
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
            return;

        if (Owner.GetPower<RetainSurgeAndDrownPower>() != null)
            return;

        await base.AfterSideTurnEnd(choiceContext, side, participants);
    }
}
