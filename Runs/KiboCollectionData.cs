using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.RunData;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Runs;

public sealed class KiboCollectionData
{
    public HashSet<string> OwnedKiboTypeIds { get; set; } = [];
    public string? ActiveKiboTypeId { get; set; }
    public string? StarterKiboTypeId { get; set; }
    public bool HasChosenStarterKibo { get; set; }
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

    public static string? GetStarterKiboTypeId(Player player)
    {
        return ResolveStarterKiboTypeId(Get(player));
    }

    public static string? ResolveStarterKiboTypeId(KiboCollectionData? data)
    {
        if (data == null) return null;

        var ownedTypeIds = data.OwnedKiboTypeIds
            .Select(id => KiboTypeId.TryParse(id, out var stem) ? stem : null)
            .Where(id => id != null)
            .Select(id => id!)
            .ToHashSet();

        if (KiboTypeId.TryParse(data.StarterKiboTypeId ?? string.Empty, out var storedTypeId) &&
            ownedTypeIds.Contains(storedTypeId))
            return storedTypeId;

        foreach (var starterDef in KiboTypeRegistry.All.Where(def => def.IsStarter))
        {
            var chainTypeId = ResolveOwnedStarterChainType(starterDef.TypeId, ownedTypeIds);
            if (chainTypeId != null)
                return chainTypeId;
        }

        return null;
    }

    private static string? ResolveOwnedStarterChainType(string starterTypeId, HashSet<string> ownedTypeIds)
    {
        string? latestOwnedTypeId = null;
        var visited = new HashSet<string>();
        var currentTypeId = starterTypeId;

        while (visited.Add(currentTypeId))
        {
            if (ownedTypeIds.Contains(currentTypeId))
                latestOwnedTypeId = currentTypeId;

            var def = KiboTypeRegistry.Get(currentTypeId);
            if (def.EvolvesTo is not { } evolvedTypeId)
                break;

            currentTypeId = evolvedTypeId;
        }

        return latestOwnedTypeId;
    }

    public static void Modify(Player player, Action<KiboCollectionData> mutate)
    {
        _data!.Modify(player, mutate);
    }
}
