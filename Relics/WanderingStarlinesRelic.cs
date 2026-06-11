using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Hooks;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
public class WanderingStarlinesRelic : StarbornRelic, IAutoTriggerListener, ITuningOverloadListener
{
    private bool _hasTriggeredThisTurn;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.ElementMark(1, Element.SealElementType.Any, "PrimaryMark"),
        StarbornCardVars.ElementMark(1, Element.SealElementType.Any, "SecondaryMark"),
        new EnergyVar(1)
    ];

    public async Task AfterAutoTrigger(PlayerChoiceContext ctx, bool anyTriggered)
    {
        if (!anyTriggered)
        {
            Flash();
            await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Primary, Owner,
                DynamicVars["PrimaryMark"].IntValue);
            await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Secondary, Owner,
                DynamicVars["SecondaryMark"].IntValue);
        }
    }

    public async Task AfterTuning(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        if (!_hasTriggeredThisTurn)
        {
            Flash();
            _hasTriggeredThisTurn = true;
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }

    public async Task AfterOverload(PlayerChoiceContext ctx, MarkSlot slot, int consume, CardModel? source)
    {
        if (!_hasTriggeredThisTurn)
        {
            Flash();
            _hasTriggeredThisTurn = true;
            await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);
        }
    }

    public override async Task BeforeSideTurnEndEarly(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner.Creature))
            _hasTriggeredThisTurn = false;
    }
}
