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
/// Applied to the player by GorgeousBody. Prevents <see cref="ExposePower"/> on enemies
/// from being consumed on attack. At end of player turn, removes all Expose from enemies
/// and expires.
/// </summary>
[RegisterPower]
public class GorgeousBodyPower : StarbornPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner))
            return;

        // Remove all Expose from enemies at end of turn
        var enemies = Owner.CombatState?.Enemies.Where(e => e.IsAlive).ToList();
        if (enemies != null)
        {
            foreach (var enemy in enemies)
            {
                var expose = enemy.GetPower<ExposePower>();
                if (expose != null)
                    await PowerCmd.Remove(expose);
            }
        }

        await PowerCmd.Remove(this);
    }
}
