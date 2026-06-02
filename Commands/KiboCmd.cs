using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.RunRngs;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Commands;

/// <summary>
/// 奇波指令入口 —— 自动出牌、切换奇波等操作统一走这里。
/// 所有对外方法都会触发对应的 Hook 事件，不要绕过此类直接操作 KiboPileManager。
/// </summary>
public static class KiboCmd
{
    /// <summary>
    /// 自动打出一张奇波牌（技能牌或绝技牌）。会先检查是否有 Listener 阻止自动出牌。
    /// </summary>
    public static async Task AutoPlay(PlayerChoiceContext ctx, CardModel card, ICombatState combatState)
    {
        if (KiboHooks.AnyListenerPreventsKiboAutoPlay(combatState, card))
            return;
        await KiboHooks.BeforeKiboCardAutoPlay(combatState, card);
        await CardCmd.AutoPlay(ctx, card, null);
        await KiboHooks.AfterKiboCardAutoPlay(combatState, card);
    }

    /// <summary>
    /// 从当前活跃牌堆中随机选一张普通能力牌并自动打出。
    /// </summary>
    public static async Task<bool> TryAutoPlayRandomNormalCard(
        Player player, ICombatState combatState) =>
        await TryAutoPlayRandomCard(player, combatState, KiboKeywords.NormalKeyword);

    /// <summary>
    /// 从当前活跃牌堆中随机选一张绝技牌并自动打出。
    /// </summary>
    public static async Task<bool> TryAutoPlayRandomUltimateCard(
        Player player, ICombatState combatState) =>
        await TryAutoPlayRandomCard(player, combatState, KiboKeywords.UltimateKeyword);

    /// <summary>
    /// 换下旧奇波（带钩子）。将指定类型从活跃堆退回后备堆。
    /// 先检查 AnyListenerPreventsKiboSwitchOff，再触发 Before/After 钩子。
    /// </summary>
    public static async Task SwitchOff(PlayerChoiceContext ctx, Player player, string typeId)
    {
        var combatState = player.Creature.CombatState;
        if (combatState != null)
        {
            if (KiboHooks.AnyListenerPreventsKiboSwitchOff(combatState, player, typeId))
                return;
            await KiboHooks.BeforeKiboSwitchOff(combatState, player, typeId);
        }

        await KiboPileManager.MoveKiboToStorage(player, typeId);

        if (combatState != null)
            await KiboHooks.AfterKiboSwitchOff(combatState, player, typeId);
    }

    /// <summary>
    /// 换上新奇波（带钩子）。将指定类型从后备堆移入活跃堆。已在活跃中的类型跳过（幂等）。
    /// 先检查 AnyListenerPreventsKiboSwitchOn，再触发 Before/After 钩子。
    /// </summary>
    public static async Task SwitchOn(PlayerChoiceContext ctx, Player player, string typeId)
    {
        var combatState = player.Creature.CombatState;
        if (combatState != null)
        {
            if (KiboHooks.AnyListenerPreventsKiboSwitchOn(combatState, player, typeId))
                return;
            await KiboHooks.BeforeKiboSwitchOn(combatState, player, typeId);
        }

        await KiboPileManager.MoveKiboToActive(player, typeId);

        if (combatState != null)
            await KiboHooks.AfterKiboSwitchOn(combatState, player, typeId);
    }

    /// <summary>
    /// 将战斗中所有可用奇波的能力牌移入活跃堆（只换上不换下）。
    /// 从 combat storage 中扫描 RepCard 获取类型，每类型走 SwitchOn 触发钩子。
    /// </summary>
    public static async Task SwitchOnAll(PlayerChoiceContext ctx, Player player)
    {
        var combatStorage = KiboPileManager.GetStorageCombatPile(player);
        if (combatStorage == null) return;

        foreach (var typeId in KiboPileManager.GetKiboTypesInPile(combatStorage))
            await SwitchOn(ctx, player, typeId);
    }

    /// <summary>
    /// 将活跃奇波切换为指定类型。组合 SwitchOff + SwitchOn，
    /// 触发 BeforeKiboSwitch / AfterKiboSwitch 钩子，
    /// 并检查 AnyListenerPreventsKiboSwitch 是否阻止切换。
    /// </summary>
    public static async Task Summon(
        PlayerChoiceContext ctx, Player player, string typeId)
    {
        // 当前活跃的奇波类型
        var from = KiboPileManager.GetActiveKiboType(player);
        if (from == typeId) return;

        // 切换前：检查阻止 + 触发 Before hook
        var combatState = player.Creature.CombatState;
        if (from != null && combatState != null)
        {
            if (KiboHooks.AnyListenerPreventsKiboSwitch(combatState, player, from, typeId))
                return;
            await KiboHooks.BeforeKiboSwitch(combatState, player, from, typeId);
        }

        if (!CombatManager.Instance.IsInProgress)
            return;

        // 换下旧 + 换上新
        if (from != null)
            await SwitchOff(ctx, player, from);
        await SwitchOn(ctx, player, typeId);

        // 切换后：触发 After hook
        if (from != null && combatState != null)
            await KiboHooks.AfterKiboSwitch(combatState, player, from, typeId);
    }

    /// <summary>
    /// 随机切换为后备牌堆中一只与当前不同的奇波。
    /// 返回 false 表示没有可供切换的后备奇波（例如只有一只或没有奇波）。
    /// </summary>
    public static async Task<bool> SummonRandom(PlayerChoiceContext ctx, Player player)
    {
        // 收集中 → 排除当前 → 随机选 → 切过去
        var combatStorage = KiboPileManager.GetStorageCombatPile(player);
        if (combatStorage == null) return false;

        var candidates = KiboPileManager.GetKiboTypesInPile(combatStorage).ToList();
        if (candidates.Count == 0) return false;

        var activeType = KiboPileManager.GetActiveKiboType(player);
        if (activeType != null) candidates.Remove(activeType);
        if (candidates.Count == 0) return false;

        var typeId = candidates[ModRunRngRegistry.Get(player, Const.ModId, "kibo_summon_random").NextInt(0, candidates.Count)];
        await Summon(ctx, player, typeId);
        return true;
    }

    /// <summary>
    /// 从活跃牌堆中随机选一张带指定 keyword 的牌，触发 hook 后自动打出。
    /// </summary>
    private static async Task<bool> TryAutoPlayRandomCard(
        Player player, ICombatState combatState, CardKeyword keyword)
    {
        var pile = KiboPileManager.GetActivePile(player);
        if (pile == null) return false;

        var cards = pile.Cards
            .Where(c => c.HasModKeyword(keyword))
            .ToList();
        if (cards.Count == 0) return false;

        var card = cards[ModRunRngRegistry.Get(player, Const.ModId, "kibo_auto_play").NextInt(0, cards.Count)];

        var keywordId = keyword.GetModKeywordId();
        await KiboHooks.BeforeKiboRandomAutoPlay(combatState, card, keywordId);
        await AutoPlay(new BlockingPlayerChoiceContext(), card, combatState);
        await KiboHooks.AfterKiboRandomAutoPlay(combatState, card, keywordId);

        return true;
    }
}
