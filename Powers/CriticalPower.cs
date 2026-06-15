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
using STS2_Starborn.Combat;
using STS2_Starborn.Element;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Powers;

[RegisterPower]
public class CriticalPower : StarbornPower, IElementMarkModifier, ISealElementMarkListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.None;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    // ── IElementMarkModifier ──

    public int ModifyTuningConsume(MarkSlot slot, int consume) => 0;
    public int ModifyOverloadConsume(MarkSlot slot, int consume) => 0;
    public bool ShouldPreventMarkRemoval(MarkSlot slot) => true;

    // ── ISealElementMarkListener ──

    public bool ShouldPreventElementChange(MarkSlot slot, SealElementType from, SealElementType to) =>
        to == SealElementType.None;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;

        var combatState = Owner.CombatState;
        if (combatState == null) return;

        var player = combatState.Players.FirstOrDefault(p => p.Creature == Owner);
        if (player == null) return;

        // Bypass hook checks (use ElementMarkManager directly, since this power
        // itself would otherwise block SetElementType → None and RemoveElementMarks)
        ElementMarkState.SetElementType(player, MarkSlot.Primary, SealElementType.None);
        ElementMarkState.SetStacks(player, MarkSlot.Primary, 0);
        ElementMarkState.SetElementType(player, MarkSlot.Secondary, SealElementType.None);
        ElementMarkState.SetStacks(player, MarkSlot.Secondary, 0);

        Flash();
        await PowerCmd.Remove(this);
    }
}
