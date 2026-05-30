using System;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2_Starborn.Element;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Combat;

public enum MarkSlot { Primary, Secondary }

public static class ElementMarkState
{
    public const int MaxSealStacks = 5;
    public const int ThresholdStacks = 3;

    public static event Action? MarksChanged;
    public static void NotifyMarksChanged() => MarksChanged?.Invoke();

    private static ElementMarkData GetData(Player player) => ElementMarkRunData.Get(player);

    public static int GetStacks(Player player, MarkSlot slot)
    {
        var data = GetData(player);
        return slot == MarkSlot.Primary ? data.PrimaryStacks : data.SecondaryStacks;
    }

    public static SealElementType GetElementType(Player player, MarkSlot slot)
    {
        var data = GetData(player);
        var raw = slot == MarkSlot.Primary ? data.PrimaryElementType : data.SecondaryElementType;
        return raw != null && Enum.TryParse<SealElementType>(raw, out var t) ? t : SealElementType.None;
    }

    public static void SetStacks(Player player, MarkSlot slot, int stacks)
    {
        stacks = Math.Clamp(stacks, 0, MaxSealStacks);
        ElementMarkRunData.Modify(player, data =>
        {
            if (slot == MarkSlot.Primary)
                data.PrimaryStacks = stacks;
            else
                data.SecondaryStacks = stacks;
        });
        NotifyMarksChanged();
    }

    public static void SetElementType(Player player, MarkSlot slot, SealElementType elementType)
    {
        ElementMarkRunData.Modify(player, data =>
        {
            var raw = elementType.ToString();
            if (slot == MarkSlot.Primary)
                data.PrimaryElementType = raw;
            else
                data.SecondaryElementType = raw;
        });
        NotifyMarksChanged();
    }
}
