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

    private int _primaryStacks;
    private string? _primaryElementType;
    private int _secondaryStacks;
    private string? _secondaryElementType;

    private int _tuningTotalCount;
    private int _overloadTotalCount;

    private bool _triggeredThisTurnStart;

    private readonly Dictionary<ulong, int> _switchCounts = [];
    private readonly Dictionary<ulong, HashSet<SealElementType>> _switchedTypes = [];
    private readonly HashSet<SealElementType> _firstOverloaded = [];

    public ElementMarkManager() : base(HookType.Combat)
    {
        _instance = this;
    }

    // ── Singleton ──

    private static ElementMarkManager _instance = null!;
    internal static ElementMarkManager Instance => _instance;

    // ── Marks data (instance, combat-scoped) ──

    public int GetStacks(MarkSlot slot) =>
        slot == MarkSlot.Primary ? _primaryStacks : _secondaryStacks;

    public void SetStacks(MarkSlot slot, int stacks)
    {
        stacks = Math.Clamp(stacks, 0, MaxSealStacks);
        if (slot == MarkSlot.Primary)
            _primaryStacks = stacks;
        else
            _secondaryStacks = stacks;
        ElementMarkState.NotifyMarksChanged();
    }

    public SealElementType GetElementType(MarkSlot slot)
    {
        var raw = slot == MarkSlot.Primary ? _primaryElementType : _secondaryElementType;
        return raw != null && System.Enum.TryParse<SealElementType>(raw, out var t) ? t : SealElementType.None;
    }

    public void SetElementType(MarkSlot slot, SealElementType elementType)
    {
        var raw = elementType.ToString();
        if (slot == MarkSlot.Primary)
            _primaryElementType = raw;
        else
            _secondaryElementType = raw;
        ElementMarkState.NotifyMarksChanged();
    }

    // ── Switch tracking ──

    public static int GetSwitchCount(Player player) =>
        Instance._switchCounts.GetValueOrDefault(player.NetId);

    public static int GetSwitchedTypeCount(Player player) =>
        Instance._switchedTypes.GetValueOrDefault(player.NetId)?.Count ?? 0;

    public static void RecordSwitch(Player player, SealElementType to)
    {
        var id = player.NetId;
        Instance._switchCounts[id] = Instance._switchCounts.GetValueOrDefault(id) + 1;
        if (!Instance._switchedTypes.ContainsKey(id))
            Instance._switchedTypes[id] = [];
        Instance._switchedTypes[id].Add(to);
    }

    private void ResetSwitchTracking(Player player)
    {
        _switchCounts.Remove(player.NetId);
        _switchedTypes.Remove(player.NetId);
    }

    // ── Tuning/Overload total count tracking ──

    public static int GetTuningTotalCount() =>
        Instance._tuningTotalCount;

    public static int GetOverloadTotalCount() =>
        Instance._overloadTotalCount;

    public static void RecordTuning() =>
        Instance._tuningTotalCount++;

    public static void RecordOverload() =>
        Instance._overloadTotalCount++;

    // ── Turn start trigger tracking ──

    public static bool TriggeredThisTurnStart() =>
        Instance._triggeredThisTurnStart;

    // ── First overload tracking ──

    public static bool IsFirstOverload(SealElementType element) =>
        Instance._firstOverloaded.Add(element);

    // ── Hook ──

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        var player = combatState.Players.FirstOrDefault();
        if (player == null) return;

        await ProcessAutoTrigger(player, combatState);
        Instance.ResetSwitchTracking(player);
    }

    private async Task ProcessAutoTrigger(Player player, ICombatState combatState)
    {
        var ctx = new ThrowingPlayerChoiceContext();

        // Before 钩子：开始自动触发流程
        await SealElementMarkHooks.BeforeAutoTrigger(combatState, ctx);

        _triggeredThisTurnStart = false;
        foreach (var slot in Slots)
        {
            var triggered = await TryTriggerAutoTuning(player, slot, combatState);
            if (triggered) _triggeredThisTurnStart = true;
            if (!triggered)
                await ResetElementToNone(player, slot);
        }

        // After 钩子：完成自动触发流程
        await SealElementMarkHooks.AfterAutoTrigger(combatState, ctx, _triggeredThisTurnStart);
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
