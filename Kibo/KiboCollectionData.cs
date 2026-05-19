using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.RunData;

namespace STS2_Starborn.Kibo;

public sealed class KiboCollectionData
{
    public HashSet<string> OwnedKiboTypeIds { get; set; } = [];
    public string? ActiveKiboTypeId { get; set; }
}

public static class KiboRunData
{
    private static PlayerRunSavedData<KiboCollectionData>? _data;

    public static void Initialize()
    {
        _data = RunSavedDataStore.For(Const.ModId)
            .RegisterPerPlayer<KiboCollectionData>("kibo_collection");
    }

    public static KiboCollectionData Get(Player player)
    {
        return _data!.Get(player);
    }

    public static void Modify(Player player, Action<KiboCollectionData> mutate)
    {
        _data!.Modify(player, mutate);
    }
}
