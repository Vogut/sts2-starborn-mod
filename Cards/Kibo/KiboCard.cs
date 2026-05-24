using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Cards.Kibo;

public abstract class KiboCard(
    CardType type,
    TargetType targetType
) : StarbornCard(0, type, CardRarity.Token, targetType, shouldShowInCardLibrary: true)
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: Const.Paths.KiboCardPortrait(GetType()));

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await base.AfterCombatEnd(room);

        if (room.RoomType != RoomType.Boss) return;
        if (!KiboPileManager.IsRepCardType(GetType())) return;
        if (Owner == null) return;

        var data = KiboRunData.Get(Owner);
        if (data?.ActiveKiboTypeId == null) return;
        if (!Enum.TryParse<KiboTypeId>(data.ActiveKiboTypeId, out var currentTypeId)) return;

        if (!Keywords.Contains(KiboKeywords.TypeKeyword(currentTypeId).GetModCardKeyword())) return;

        var def = KiboTypeRegistry.Get(currentTypeId);
        if (def.EvolvesTo is not { } evolvedTypeId) return;

        KiboRunData.Modify(Owner, d =>
        {
            d.ActiveKiboTypeId = evolvedTypeId.ToString();
            d.OwnedKiboTypeIds.Remove(currentTypeId.ToString());
            d.OwnedKiboTypeIds.Add(evolvedTypeId.ToString());
        });

        KiboPileManager.RemoveMasterCards(Owner, currentTypeId);
        await KiboPileManager.CreateMasterCards(Owner, evolvedTypeId);
    }
}
