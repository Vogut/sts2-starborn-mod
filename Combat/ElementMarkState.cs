using System;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Element;

namespace STS2_Starborn.Combat;

public enum MarkSlot { Primary, Secondary }

public static class ElementMarkState
{
    public const int MaxSealStacks = ElementMarkManager.MaxSealStacks;
    public const int ThresholdStacks = ElementMarkManager.ThresholdStacks;

    internal static ElementMarkManager Manager => ModelDb.Singleton<ElementMarkManager>();

    public static event Action? MarksChanged;
    public static void NotifyMarksChanged() => MarksChanged?.Invoke();

    public static int GetStacks(Player player, MarkSlot slot) =>
        Manager.GetStacks(player, slot);

    public static bool TryGetStacks(Player player, MarkSlot slot, out int stacks) =>
        Manager.TryGetStacks(player, slot, out stacks);

    public static SealElementType GetElementType(Player player, MarkSlot slot) =>
        Manager.GetElementType(player, slot);

    public static bool TryGetElementType(Player player, MarkSlot slot, out SealElementType elementType) =>
        Manager.TryGetElementType(player, slot, out elementType);

    public static void SetStacks(Player player, MarkSlot slot, int stacks) =>
        Manager.SetStacks(player, slot, stacks);

    public static void SetElementType(Player player, MarkSlot slot, SealElementType elementType) =>
        Manager.SetElementType(player, slot, elementType);
}
