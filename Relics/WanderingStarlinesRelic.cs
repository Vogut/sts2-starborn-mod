using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Hooks;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
public class WanderingStarlinesRelic : StarbornRelic, ITuningOverloadListener
{
    private bool _hasTriggeredThisTurn;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(ElementMarkState.MarkProgressThreshold),
        StarbornCardVars.ElementMark(1, Element.SealElementType.Any, "PrimaryMark"),
        StarbornCardVars.ElementMark(1, Element.SealElementType.Any, "SecondaryMark"),
        new EnergyVar(1)
    ];

    public override Task BeforeCombatStart()
    {
        ElementMarkState.ResetProgress(Owner);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState == null ||
            cardPlay.Card.Owner != Owner ||
            cardPlay.IsAutoPlay)
            return;

        if (cardPlay.Card.Type == CardType.Skill)
            await AdvanceProgress(choiceContext, MarkSlot.Primary);
        else if (cardPlay.Card.Type == CardType.Attack)
            await AdvanceProgress(choiceContext, MarkSlot.Secondary);
    }

    private async Task AdvanceProgress(PlayerChoiceContext ctx, MarkSlot slot)
    {
        var progress = ElementMarkState.GetProgress(Owner, slot) + 1;
        if (progress < DynamicVars.Cards.IntValue)
        {
            ElementMarkState.SetProgress(Owner, slot, progress);
            return;
        }

        ElementMarkState.SetProgress(Owner, slot, 0);
        Flash();
        await SealElementMarkCmd.GainElementMarks(
            ctx,
            slot,
            Owner,
            DynamicVars[slot == MarkSlot.Primary ? "PrimaryMark" : "SecondaryMark"].IntValue);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        ElementMarkState.ResetProgress(Owner);
        return Task.CompletedTask;
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
