using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Events;
using STS2_Starborn.Map;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Relics;

[RegisterRelic(typeof(StarbornRelicPool))]
[RegisterCharacterStarterRelic(typeof(Starborn))]
public class StarbornMarkRelic : StarbornRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public static MapCoord? KiboNodeCoord { get; private set; }

    public override ActMap ModifyGeneratedMap(IRunState runState, ActMap map, int actIndex)
    {
        KiboNodeCoord = null;
        if (actIndex != 0) return map;

        var wrapper = new KiboActMap(map);
        KiboNodeCoord = new MapCoord(3, 1);
        return wrapper;
    }

    private bool ShouldOverrideForKiboEvent()
    {
        if (KiboNodeCoord == null) return false;
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

    public override async Task AfterSideTurnStart(CombatSide side,
        IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != base.Owner.Creature.Side || combatState.RoundNumber != 1)
            return;

        ElementMarkRunData.Modify(base.Owner, data =>
        {
            data.PrimaryStacks = 1;
            data.SecondaryStacks = 1;
        });
        ElementMarkManager.NotifyMarksChanged();

        var data = KiboRunData.Get(base.Owner);
        if (data?.ActiveKiboTypeId == null) return;
        if (!Enum.TryParse<KiboTypeId>(data.ActiveKiboTypeId, out var typeId)) return;

        var activePile = KiboPileManager.GetActivePile(base.Owner);
        if (activePile != null && activePile.Cards.Count > 0) return;

        await KiboPileManager.ActivateType(base.Owner, typeId);
    }
}
