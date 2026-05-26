using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Powers;

[RegisterPower]
public class RubyIsTier0Power : StarbornPower, ISealElementMarkListener
{
    private int _cardsModifiedThisTurn;

    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override bool AllowNegative => false;
    public override string? elementPrefix => "Fire";

    public override PowerAssetProfile AssetProfile => new(
        IconPath: Const.Paths.PowerIcon(GetType()),
        BigIconPath: Const.Paths.PowerBigIcon(GetType())
    );

    public bool ShouldPreventElementChange(MarkSlot slot, SealElementType from, SealElementType to)
    {
        if (to == SealElementType.Fire) return false;
        return true;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner)) return;

        _cardsModifiedThisTurn = 0;

        var player = combatState.Players.FirstOrDefault(p => p.Creature == Owner);
        if (player == null) return;

        await SealElementMarkCmd.GainElementMarks(
            new ThrowingPlayerChoiceContext(), MarkSlot.Primary, player, 2);
        await SealElementMarkCmd.GainElementMarks(
            new ThrowingPlayerChoiceContext(), MarkSlot.Secondary, player, 2);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Type != CardType.Attack) return;
        if (cardPlay.Card.Owner?.Creature != Owner) return;
        if (_cardsModifiedThisTurn >= (int)Amount) return;

        _cardsModifiedThisTurn++;

        var player = cardPlay.Card.Owner;

        if (StarbornCmd.CanTuning(player, MarkSlot.Primary))
            await StarbornCmd.Tuning(choiceContext, MarkSlot.Primary, player, 1, null);
        if (StarbornCmd.CanTuning(player, MarkSlot.Secondary))
            await StarbornCmd.Tuning(choiceContext, MarkSlot.Secondary, player, 1, null);

        var target = cardPlay.Target;
        if (target == null) return;

        var burnStacks = (int)(target.Powers.OfType<BurnPower>().FirstOrDefault()?.Amount ?? 0);
        if (burnStacks > 0)
            await CreatureCmd.Damage(
                choiceContext, target, burnStacks,
                ValueProp.Unpowered, Owner, null);
    }
}
