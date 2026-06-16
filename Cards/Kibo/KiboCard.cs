using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Cards.Kibo;

public abstract class KiboCard(
    int cost,
    CardType type,
    TargetType targetType
) : StarbornCard(cost, type, CardRarity.Token, targetType, shouldShowInCardLibrary: true)
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: Const.Paths.KiboCardPortrait(GetType()));

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            if (!KiboPileManager.IsRepCardType(GetType()))
                return [];

            var def = KiboTypeRegistry.GetByRepCardType(GetType());
            return def.CreatePlayableCardHoverTips();
        }
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await base.AfterCombatEnd(room);

        if (room.RoomType != RoomType.Boss) return;
        if (!KiboPileManager.IsRepCardType(GetType())) return;
        if (Owner == null) return;

        var data = KiboRunData.Get(Owner);
        if (data?.ActiveKiboTypeId == null) return;
        if (!KiboTypeId.TryParse(data.ActiveKiboTypeId, out var currentTypeId)) return;

        if (!Keywords.Contains(KiboKeywords.TypeKeyword(currentTypeId).GetModCardKeyword())) return;

        var def = KiboTypeRegistry.Get(currentTypeId);
        if (def.EvolvesTo is not { } evolvedTypeId) return;

        KiboRunData.Modify(Owner, d =>
        {
            if (KiboRunData.ResolveStarterKiboTypeId(d) == currentTypeId)
            {
                d.StarterKiboTypeId = evolvedTypeId;
                d.HasChosenStarterKibo = true;
            }

            d.ActiveKiboTypeId = evolvedTypeId;
            d.OwnedKiboTypeIds.Remove(currentTypeId);
            d.OwnedKiboTypeIds.Add(evolvedTypeId);
        });

        KiboPileManager.RemoveMasterCards(Owner, currentTypeId);
        await KiboPileManager.CreateMasterCards(Owner, evolvedTypeId);
    }
}
