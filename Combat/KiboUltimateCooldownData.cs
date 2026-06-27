using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Utils;

namespace STS2_Starborn.Combat;

public sealed class KiboUltimateCooldownData
{
    public readonly Dictionary<string, int> RemainingTurnsByType = new();
}

public static class KiboUltimateCooldownState
{
    public const int DefaultCooldownTurns = 1;

    private static readonly AttachedState<PlayerCombatState, KiboUltimateCooldownData> Data
        = new(() => new());

    public static event Action? CooldownsChanged;

    public static int GetRemainingTurns(Player player, string kiboTypeId)
    {
        if (!TryGet(player, out var data) || data == null)
            return 0;

        return data.RemainingTurnsByType.GetValueOrDefault(kiboTypeId);
    }

    public static bool IsCoolingDown(Player player, string kiboTypeId) =>
        GetRemainingTurns(player, kiboTypeId) > 0;

    public static void StartCooldown(Player player, string kiboTypeId, int turns = DefaultCooldownTurns)
    {
        if (player.PlayerCombatState == null)
            return;

        var data = Data.GetOrCreate(player.PlayerCombatState);
        var clampedTurns = Math.Max(0, turns);
        if (clampedTurns == 0)
        {
            if (data.RemainingTurnsByType.Remove(kiboTypeId))
                CooldownsChanged?.Invoke();
            return;
        }

        data.RemainingTurnsByType[kiboTypeId] = clampedTurns;
        CooldownsChanged?.Invoke();
    }

    public static void ReduceCooldowns(Player player)
    {
        if (!TryGet(player, out var data) || data == null || data.RemainingTurnsByType.Count == 0)
            return;

        var changed = false;
        foreach (var kiboTypeId in data.RemainingTurnsByType.Keys.ToList())
        {
            var remainingTurns = data.RemainingTurnsByType[kiboTypeId] - 1;
            if (remainingTurns <= 0)
                data.RemainingTurnsByType.Remove(kiboTypeId);
            else
                data.RemainingTurnsByType[kiboTypeId] = remainingTurns;

            changed = true;
        }

        if (changed)
            CooldownsChanged?.Invoke();
    }

    private static bool TryGet(Player player, out KiboUltimateCooldownData? data)
    {
        ArgumentNullException.ThrowIfNull(player);
        data = null;

        return player.PlayerCombatState != null &&
               Data.TryGetValue(player.PlayerCombatState, out data);
    }
}
