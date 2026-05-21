using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.RunData;

namespace STS2_Starborn.Runs;

public sealed class ElementMarkData
{
    public int PrimaryStacks { get; set; }
    public string? PrimaryElementType { get; set; }
    public int SecondaryStacks { get; set; }
    public string? SecondaryElementType { get; set; }
}

public static class ElementMarkRunData
{
    private static PlayerRunSavedData<ElementMarkData>? _data;

    public static void Initialize()
    {
        _data = RunSavedDataStore.For(Const.ModId)
            .RegisterPerPlayer<ElementMarkData>("element_marks");
    }

    public static ElementMarkData Get(Player player) => _data!.Get(player);

    public static void Modify(Player player, Action<ElementMarkData> mutate) => _data!.Modify(player, mutate);
}
