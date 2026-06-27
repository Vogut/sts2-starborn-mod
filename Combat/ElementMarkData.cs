using System;
using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Utils;
using STS2_Starborn.Element;

namespace STS2_Starborn.Combat;

/// <summary>
/// 元素印记战斗数据。每场战斗自动创建新实例，战斗结束自动回收。
/// </summary>
public sealed class ElementMarkData
{
    // 元素印记核心数据
    public int PrimaryStacks;
    public string? PrimaryElementType;
    public int SecondaryStacks;
    public string? SecondaryElementType;
    public int PrimaryProgress;
    public int SecondaryProgress;

    // 计数器
    public int TuningTotalCount;
    public int OverloadTotalCount;
    public bool TriggeredThisTurnStart;

    // 追踪集合（可变容器，无需每场战斗重新分配）
    public readonly Dictionary<ulong, int> SwitchCounts = new();
    public readonly Dictionary<ulong, HashSet<SealElementType>> SwitchedTypes = new();
    public readonly HashSet<SealElementType> FirstOverloaded = new();
}

/// <summary>
/// 元素印记数据存储。使用 AttachedState 模式将数据绑定到 PlayerCombatState。
/// </summary>
public static class ElementMarkDataStore
{
    private static readonly AttachedState<PlayerCombatState, ElementMarkData> Data
        = new(() => new());

    /// <summary>
    /// 获取玩家的元素印记数据。首次访问时自动创建，战斗结束时自动释放。
    /// </summary>
    public static ElementMarkData Get(Player player)
    {
        ArgumentNullException.ThrowIfNull(player);

        if (player.PlayerCombatState == null)
            throw new InvalidOperationException(
                "Cannot get element mark data: player does not have a combat state.");

        return Data.GetOrCreate(player.PlayerCombatState);
    }

    /// <summary>
    /// 尝试获取已存在的数据，不创建新实例。用于非战斗上下文的安全查询。
    /// </summary>
    public static bool TryGet(Player player, out ElementMarkData? data)
    {
        ArgumentNullException.ThrowIfNull(player);
        data = null;

        return player.PlayerCombatState != null &&
               Data.TryGetValue(player.PlayerCombatState, out data);
    }
}
