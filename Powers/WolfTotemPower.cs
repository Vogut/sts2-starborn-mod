using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Powers;

[RegisterPower]
public class WolfTotemPower : StarbornPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5m, ValueProp.Unpowered),
        StarbornCardVars.ElementMark(2, SealElementType.Water),
    ];

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        if (!participants.Contains(Owner)) return;
        if (Owner.Player == null) return;

        Flash();
        await CreatureCmd.GainBlock(
            Owner, DynamicVars.Block.IntValue, ValueProp.Unpowered, null);

        var elementType = ((SealElementVar)DynamicVars["ElementMark"]).ElementType;
        await SealElementMarkCmd.GainElementMarks(
            new ThrowingPlayerChoiceContext(),
            MarkSlot.Primary, Owner.Player,
            DynamicVars["ElementMark"].IntValue, elementType);

        await PowerCmd.Decrement(this);
    }
}
