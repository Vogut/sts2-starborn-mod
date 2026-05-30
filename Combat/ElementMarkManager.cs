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


namespace STS2_Starborn.Combat;

[RegisterSingleton]
public sealed class ElementMarkManager : HookedSingletonModel
{
    private static readonly MarkSlot[] Slots = [MarkSlot.Primary, MarkSlot.Secondary];

    private readonly Dictionary<ulong, int> _switchCounts = [];
    private readonly Dictionary<ulong, HashSet<SealElementType>> _switchedTypes = [];
    private readonly HashSet<SealElementType> _firstOverloaded = [];

    public ElementMarkManager() : base(HookType.Combat)
    {
        _instance = this;
    }

    // ── Singleton ──

    private static ElementMarkManager _instance = null!;
    private static ElementMarkManager Instance => _instance;

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

    // ── First overload tracking ──

    public static bool IsFirstOverload(SealElementType element) =>
        Instance._firstOverloaded.Add(element);

    // ── Hook ──

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player) return;
        var player = combatState.Players.FirstOrDefault();
        if (player == null) return;

        foreach (var slot in Slots)
            await TryTriggerAutoTuning(player, slot, combatState);

        Instance.ResetSwitchTracking(player);

        foreach (var slot in Slots)
            await ResetElementToNone(player, slot);
    }

    private static async Task TryTriggerAutoTuning(Player player, MarkSlot slot, ICombatState combatState)
    {
        var elementType = ElementMarkState.GetElementType(player, slot);
        if (elementType == SealElementType.None) return;

        var stacks = ElementMarkState.GetStacks(player, slot);
        var element = StarbornElement.For(elementType);

        if (stacks >= ElementMarkState.MaxSealStacks)
            await StarbornCmd.Overload(new ThrowingPlayerChoiceContext(), slot, player, element.OverloadConsume);
        else if (stacks >= ElementMarkState.ThresholdStacks)
            await StarbornCmd.Tuning(new ThrowingPlayerChoiceContext(), slot, player, element.TuningConsume);
    }

    private static async Task ResetElementToNone(Player player, MarkSlot slot)
    {
        await SealElementMarkCmd.SetElementType(
            new ThrowingPlayerChoiceContext(), slot, player, SealElementType.None);
    }
}
