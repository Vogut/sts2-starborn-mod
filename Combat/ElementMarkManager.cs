using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using STS2_Starborn.Commands;
using STS2_Starborn.Hooks;
using STS2_Starborn.Element;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Combat;

public enum MarkSlot { Primary, Secondary }

[RegisterSingleton]
public sealed class ElementMarkManager : HookedSingletonModel
{
    public const int MaxSealStacks = 5;
    public const int ThresholdStacks = 3;

    public static event Action? MarksChanged;

    public ElementMarkManager() : base(HookedSingletonModel.HookType.Combat) { }

    public static void NotifyMarksChanged() => MarksChanged?.Invoke();

    // ── Accessors ──

    public static ElementMarkData GetData(Player player) => ElementMarkRunData.Get(player);

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

    // ── Hooks ──

    public override Task AfterSideTurnStart(CombatSide side, ICombatState combatState)
    {
        if (side != CombatSide.Player) return Task.CompletedTask;
        var player = combatState.Players.FirstOrDefault();
        if (player == null) return Task.CompletedTask;

        foreach (var slot in new[] { MarkSlot.Primary, MarkSlot.Secondary })
        {
            var prev = GetElementType(player, slot);
            if (prev != SealElementType.None
                && !SealElementMarkHooks.AnyListenerPreventsElementChange(combatState, slot, prev, SealElementType.None))
            {
                SetElementType(player, slot, SealElementType.None);
            }
        }
        return Task.CompletedTask;
    }

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (side != CombatSide.Player) return;
        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null) return;
        var player = combatState.Players.FirstOrDefault();
        if (player == null) return;

        foreach (var slot in new[] { MarkSlot.Primary, MarkSlot.Secondary })
        {
            var stacks = GetStacks(player, slot);
            var elementType = GetElementType(player, slot);
            if (elementType == SealElementType.None) continue;

            var elementPower = Element.StarbornElement.For(elementType);
            var tuningConsume = SealElementMarkHooks.ModifyTuningConsume(combatState, slot, elementPower.TuningConsume);
            var overloadConsume = SealElementMarkHooks.ModifyOverloadConsume(combatState, slot, elementPower.OverloadConsume);

            if (stacks >= MaxSealStacks)
            {
                await StarbornCmd.Overload(choiceContext, slot, player, overloadConsume);
            }
            else if (stacks >= ThresholdStacks)
            {
                await StarbornCmd.Tuning(choiceContext, slot, player, tuningConsume);
            }
        }
    }
}
