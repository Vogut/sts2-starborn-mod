using System.Linq;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Events;

[RegisterSharedEvent]
public sealed class KiboStarterEvent : ModEventTemplate
{
    public override bool IsAllowed(IRunState runState) => false;

    public override bool IsShared => true;
    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://images/events/battleworn_dummy.png"
    );
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return KiboTypeRegistry.All
            .Where(def => def.IsStarter)
            .Select(def =>
            {
                var repCardModel = ModelDb.GetById<CardModel>(ModelDb.GetId(def.RepCardType));
                return new EventOption(
                    this,
                    () => SelectKibo(def.TypeId),
                    InitialOptionKey(ToOptionKey(def.TypeId)),
                    HoverTipFactory.FromCard(repCardModel));
            })
            .ToList();
    }

    private async Task SelectKibo(string typeId)
    {
        KiboRunData.Modify(Owner!, d =>
        {
            d.ActiveKiboTypeId = typeId;
            d.OwnedKiboTypeIds.Add(typeId);
        });
        await KiboPileManager.CreateMasterCards(Owner!, typeId);
        SetEventFinished(PageDescription("DONE"));
    }

    private static string ToOptionKey(string typeId) =>
        typeId.Replace("_", " ").ToUpperInvariant();
}
