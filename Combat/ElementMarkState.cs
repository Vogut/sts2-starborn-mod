using System;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2_Starborn.Element;

namespace STS2_Starborn.Combat;

public enum MarkSlot { Primary, Secondary }

public static class ElementMarkState
{
    public const int MaxSealStacks = ElementMarkManager.MaxSealStacks;
    public const int ThresholdStacks = ElementMarkManager.ThresholdStacks;

    private static ElementMarkManager? _manager;
    internal static ElementMarkManager Manager => _manager ??= new ElementMarkManager();

    public static event Action? MarksChanged;
    public static void NotifyMarksChanged() => MarksChanged?.Invoke();

    public static int GetStacks(Player player, MarkSlot slot) =>
        Manager.GetStacks(player, slot);

    public static SealElementType GetElementType(Player player, MarkSlot slot) =>
        Manager.GetElementType(player, slot);

    public static void SetStacks(Player player, MarkSlot slot, int stacks) =>
        Manager.SetStacks(player, slot, stacks);

    public static void SetElementType(Player player, MarkSlot slot, SealElementType elementType) =>
        Manager.SetElementType(player, slot, elementType);
}
