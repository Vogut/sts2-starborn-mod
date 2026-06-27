using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Event;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Events;
using STS2_Starborn.Hooks;
using STS2_Starborn.Map;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
[RegisterCharacterStarterRelic(typeof(Starborn))]
public class StarBoundCardRelic : StarbornRelic, IAutoTriggerListener
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(ElementMarkState.MarkProgressThreshold),
        StarbornCardVars.ElementMark(1, SealElementType.Any, "PrimaryMark"),
        StarbornCardVars.ElementMark(1, SealElementType.Any, "SecondaryMark")
    ];

    public override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
    {
        if (actIndex != 0) return map;
        return new KiboActMap(map);
    }

    // LanternKey pattern: condition based on persistent state, not static coord
    private bool ShouldOverrideForKiboEvent()
    {
        if (base.Owner.RunState.CurrentActIndex != 0) return false;
        var data = KiboRunData.Get(base.Owner);
        return data == null || (!data.HasChosenStarterKibo && KiboRunData.ResolveStarterKiboTypeId(data) == null);
    }

    public override IReadOnlySet<RoomType> ModifyUnknownMapPointRoomTypes(IReadOnlySet<RoomType> roomTypes)
    {
        if (!ShouldOverrideForKiboEvent()) return roomTypes;
        return new HashSet<RoomType> { RoomType.Event };
    }

    public override EventModel ModifyNextEvent(EventModel currentEvent)
    {
        if (!ShouldOverrideForKiboEvent()) return currentEvent;
        return ModelDb.Event<KiboStarterEvent>();
    }
    public override async Task BeforeHandDraw(
        Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != base.Owner || base.Owner.PlayerCombatState?.TurnNumber != 1)
            return;

        var typeId = KiboRunData.GetStarterKiboTypeId(base.Owner);
        if (typeId == null) return;

        var summonCard = combatState.CreateCard<SummonStarterKiboCard>(base.Owner);
        await CardPileCmd.AddGeneratedCardsToCombat(
            new List<CardModel> { summonCard }, PileType.Hand, base.Owner);
    }

    public override Task BeforeCombatStart()
    {
        ElementMarkState.ResetProgress(base.Owner);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.Owner.PlayerCombatState == null ||
            cardPlay.Card.Owner != base.Owner ||
            cardPlay.IsAutoPlay)
            return;

        if (cardPlay.Card.Type == CardType.Skill)
            await AdvanceProgress(choiceContext, MarkSlot.Primary);
        else if (cardPlay.Card.Type == CardType.Attack)
            await AdvanceProgress(choiceContext, MarkSlot.Secondary);
    }

    private async Task AdvanceProgress(PlayerChoiceContext ctx, MarkSlot slot)
    {
        var progress = ElementMarkState.GetProgress(base.Owner, slot) + 1;
        if (progress < DynamicVars.Cards.IntValue)
        {
            ElementMarkState.SetProgress(base.Owner, slot, progress);
            return;
        }

        ElementMarkState.SetProgress(base.Owner, slot, 0);
        Flash();
        await SealElementMarkCmd.GainElementMarks(
            ctx,
            slot,
            base.Owner,
            DynamicVars[slot == MarkSlot.Primary ? "PrimaryMark" : "SecondaryMark"].IntValue);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        ElementMarkState.ResetProgress(base.Owner);
        return Task.CompletedTask;
    }

    // IAutoTriggerListener 实现
    public async Task AfterAutoTrigger(PlayerChoiceContext ctx, bool anyTriggered)
    {
        if (base.Owner.PlayerCombatState?.TurnNumber != 1)
            return;

        var typeId = KiboRunData.GetStarterKiboTypeId(base.Owner);
        if (typeId == null) return;

        await KiboCmd.Summon(ctx, base.Owner, typeId);
    }
}
