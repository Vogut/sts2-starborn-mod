using System;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Element;

namespace STS2_Starborn.Combat;

public enum MarkSlot { Primary, Secondary }

public enum MarkVisualChangeKind
{
    ElementChanged,
    StacksGained,
    StacksLost,
    Tuned,
    Overloaded,
}

public readonly record struct MarkVisualChange(
    Player Player,
    MarkSlot Slot,
    SealElementType OldElementType,
    SealElementType NewElementType,
    int OldStacks,
    int NewStacks,
    MarkVisualChangeKind Kind);

public readonly record struct MarkProgressChange(
    Player Player,
    MarkSlot Slot,
    int OldProgress,
    int NewProgress,
    int Threshold);

public static class ElementMarkState
{
    public const int MaxSealStacks = ElementMarkManager.MaxSealStacks;
    public const int ThresholdStacks = ElementMarkManager.ThresholdStacks;
    public const int MarkProgressThreshold = ElementMarkManager.MarkProgressThreshold;

    internal static ElementMarkManager Manager => ModelDb.Singleton<ElementMarkManager>();

    public static event Action? MarksChanged;
    public static void NotifyMarksChanged() => MarksChanged?.Invoke();

    public static event Action<MarkVisualChange>? MarkVisualChanged;
    public static void NotifyMarkVisualChanged(MarkVisualChange change) => MarkVisualChanged?.Invoke(change);

    public static event Action<MarkProgressChange>? MarkProgressChanged;
    public static void NotifyMarkProgressChanged(MarkProgressChange change) => MarkProgressChanged?.Invoke(change);

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

    public static int GetProgress(Player player, MarkSlot slot) =>
        Manager.GetProgress(player, slot);

    public static bool TryGetProgress(Player player, MarkSlot slot, out int progress) =>
        Manager.TryGetProgress(player, slot, out progress);

    public static void SetProgress(Player player, MarkSlot slot, int progress) =>
        Manager.SetProgress(player, slot, progress);

    public static void ResetProgress(Player player) =>
        Manager.ResetProgress(player);
}
