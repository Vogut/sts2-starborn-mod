using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
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
        return data?.ActiveKiboTypeId == null;
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
    public override async Task BeforeCombatStart()
    {
        var data = KiboRunData.Get(base.Owner);
        if (data?.ActiveKiboTypeId == null) return;
        if (!KiboTypeId.TryParse(data.ActiveKiboTypeId, out var typeId)) return;

        var activePile = KiboPileManager.GetActivePile(base.Owner);
        if (activePile != null && activePile.Cards.Count > 0) return;

        // 1. 保留原有的直接召唤逻辑
        await KiboCmd.Summon(new BlockingPlayerChoiceContext(), base.Owner, typeId);

        // 2. 添加召唤卡到手牌
        var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId<Cards.Event.SummonStarterKiboCard>());
        var combatState = base.Owner.Creature.CombatState;
        if (combatState != null)
        {
            var summonCard = combatState.CreateCard(canonical, base.Owner);
            await CardPileCmd.AddGeneratedCardToCombat(summonCard, PileType.Hand, base.Owner);
        }
    }

    // IAutoTriggerListener 实现
    public async Task AfterAutoTrigger(PlayerChoiceContext ctx, bool anyTriggered)
    {
        // 若本回合开始时未触发调谐/超限，给主副属性各+1层（包括 None）
        if (!anyTriggered)
        {
            await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Primary, base.Owner,
                DynamicVars["PrimaryMark"].IntValue);
            await SealElementMarkCmd.GainElementMarks(ctx, MarkSlot.Secondary, base.Owner,
                DynamicVars["SecondaryMark"].IntValue);
        }
    }
}
