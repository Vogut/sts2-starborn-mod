using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Combat;

[RegisterSingleton]
public sealed class ElementMarkManager : HookedSingletonModel
{
    public const int MaxSealStacks = 5;
    public const int ThresholdStacks = 3;

    private static readonly MarkSlot[] Slots = [MarkSlot.Primary, MarkSlot.Secondary];

    public ElementMarkManager() : base(HookType.Combat) { }

    // ── Marks data (per-combat via ElementMarkDataStore) ──

    public int GetStacks(Player player, MarkSlot slot)
    {
        var data = ElementMarkDataStore.Get(player);
        return slot == MarkSlot.Primary ? data.PrimaryStacks : data.SecondaryStacks;
    }

    public void SetStacks(Player player, MarkSlot slot, int stacks)
    {
        var data = ElementMarkDataStore.Get(player);
        stacks = Math.Clamp(stacks, 0, MaxSealStacks);
        if (slot == MarkSlot.Primary)
            data.PrimaryStacks = stacks;
        else
            data.SecondaryStacks = stacks;
        ElementMarkState.NotifyMarksChanged();
    }

    public SealElementType GetElementType(Player player, MarkSlot slot)
    {
        var data = ElementMarkDataStore.Get(player);
        var raw = slot == MarkSlot.Primary ? data.PrimaryElementType : data.SecondaryElementType;
        return raw != null && System.Enum.TryParse<SealElementType>(raw, out var t) ? t : SealElementType.None;
    }

    public void SetElementType(Player player, MarkSlot slot, SealElementType elementType)
    {
        var data = ElementMarkDataStore.Get(player);
        var raw = elementType.ToString();
        if (slot == MarkSlot.Primary)
            data.PrimaryElementType = raw;
        else
            data.SecondaryElementType = raw;
        ElementMarkState.NotifyMarksChanged();
    }

    // ── Switch tracking ──

    public static int GetSwitchCount(Player player)
    {
        var data = ElementMarkDataStore.Get(player);
        return data.SwitchCounts.GetValueOrDefault(player.NetId);
    }

    public static int GetSwitchedTypeCount(Player player)
    {
        var data = ElementMarkDataStore.Get(player);
        return data.SwitchedTypes.GetValueOrDefault(player.NetId)?.Count ?? 0;
    }

    public static void RecordSwitch(Player player, SealElementType to)
    {
        var data = ElementMarkDataStore.Get(player);
        var id = player.NetId;
        data.SwitchCounts[id] = data.SwitchCounts.GetValueOrDefault(id) + 1;
        if (!data.SwitchedTypes.ContainsKey(id))
            data.SwitchedTypes[id] = [];
        data.SwitchedTypes[id].Add(to);
    }

    private static void ResetSwitchTracking(Player player)
    {
        var data = ElementMarkDataStore.Get(player);
        data.SwitchCounts.Remove(player.NetId);
        data.SwitchedTypes.Remove(player.NetId);
    }

    // ── Tuning/Overload total count tracking ──

    public static int GetTuningTotalCount(Player player)
    {
        var data = ElementMarkDataStore.Get(player);
        return data.TuningTotalCount;
    }

    public static int GetOverloadTotalCount(Player player)
    {
        var data = ElementMarkDataStore.Get(player);
        return data.OverloadTotalCount;
    }

    public static void RecordTuning(Player player)
    {
        var data = ElementMarkDataStore.Get(player);
        data.TuningTotalCount++;
    }

    public static void RecordOverload(Player player)
    {
        var data = ElementMarkDataStore.Get(player);
        data.OverloadTotalCount++;
    }

    // ── Turn start trigger tracking ──

    public static bool TriggeredThisTurnStart(Player player)
    {
        var data = ElementMarkDataStore.Get(player);
        return data.TriggeredThisTurnStart;
    }

    // ── First overload tracking ──

    public static bool IsFirstOverload(Player player, SealElementType element)
    {
        var data = ElementMarkDataStore.Get(player);
        return data.FirstOverloaded.Add(element);
    }

    // ── Hook ──

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        var player = combatState.Players.FirstOrDefault();
        if (player == null) return;

        await ProcessAutoTrigger(player, combatState);
        ResetSwitchTracking(player);
    }

    private static async Task ProcessAutoTrigger(Player player, ICombatState combatState)
    {
        var ctx = new ThrowingPlayerChoiceContext();

        // Before 钩子：开始自动触发流程
        await SealElementMarkHooks.BeforeAutoTrigger(combatState, ctx);

        var data = ElementMarkDataStore.Get(player);
        data.TriggeredThisTurnStart = false;
        foreach (var slot in Slots)
        {
            var triggered = await TryTriggerAutoTuning(player, slot, combatState);
            if (triggered) data.TriggeredThisTurnStart = true;
            if (!triggered)
                await ResetElementToNone(player, slot);
        }

        // After 钩子：完成自动触发流程
        await SealElementMarkHooks.AfterAutoTrigger(combatState, ctx, data.TriggeredThisTurnStart);
    }

    private static async Task<bool> TryTriggerAutoTuning(Player player, MarkSlot slot, ICombatState combatState)
    {
        var elementType = ElementMarkState.GetElementType(player, slot);
        if (elementType == SealElementType.None) return false;

        var stacks = ElementMarkState.GetStacks(player, slot);
        var element = StarbornElement.For(elementType);
        var ctx = new ThrowingPlayerChoiceContext();

        if (stacks >= MaxSealStacks)
        {
            await StarbornCmd.Overload(ctx, slot, player, element.OverloadConsume);
            return true;
        }

        if (stacks >= ThresholdStacks)
        {
            await StarbornCmd.Tuning(ctx, slot, player, element.TuningConsume);
            return true;
        }

        return false;
    }

    private static async Task ResetElementToNone(Player player, MarkSlot slot)
    {
        await SealElementMarkCmd.SetElementType(
            new ThrowingPlayerChoiceContext(), slot, player, SealElementType.None);
    }
}
