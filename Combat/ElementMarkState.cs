using System;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2_Starborn.Element;

namespace STS2_Starborn.Combat;

public enum MarkSlot { Primary, Secondary }

public static class ElementMarkState
{
    public const int MaxSealStacks = ElementMarkManager.MaxSealStacks;
    public const int ThresholdStacks = ElementMarkManager.ThresholdStacks;

    public static event Action? MarksChanged;
    public static void NotifyMarksChanged() => MarksChanged?.Invoke();

    public static int GetStacks(Player player, MarkSlot slot) =>
        ElementMarkManager.Instance.GetStacks(slot);

    public static SealElementType GetElementType(Player player, MarkSlot slot) =>
        ElementMarkManager.Instance.GetElementType(slot);

    public static void SetStacks(Player player, MarkSlot slot, int stacks) =>
        ElementMarkManager.Instance.SetStacks(slot, stacks);

    public static void SetElementType(Player player, MarkSlot slot, SealElementType elementType) =>
        ElementMarkManager.Instance.SetElementType(slot, elementType);
}
