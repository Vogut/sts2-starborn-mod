using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Models;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Commands;
using STS2_Starborn.Hooks;

namespace STS2_Starborn.Combat;

/// <summary>
/// 奇波战斗管理器。负责战斗初始化、回合末自动出牌、以及奇波牌结算后的牌堆路由。
/// </summary>
[RegisterSingleton]
public sealed class KiboCombatManager : HookedSingletonModel, IKiboCardPlayListener
{
    public KiboCombatManager() : base(HookType.Combat) { }

    /// <summary>
    /// 战斗开始时，为每位玩家初始化奇波后备牌堆（从 Storage 克隆到 StorageCombat）。
    /// </summary>
    public override async Task BeforeCombatStart()
    {
        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null) return;

        foreach (var player in combatState.Players)
            await KiboPileManager.InitializeForCombat(player);
    }

    public override Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != CombatSide.Player)
            return Task.CompletedTask;

        foreach (var player in combatState.Players)
            KiboUltimateCooldownState.ReduceCooldowns(player);

        return Task.CompletedTask;
    }

    /// <summary>
    /// 玩家回合结束时，从活跃牌堆中随机自动打出一张普通战技牌。
    /// </summary>
    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
            return;

        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null)
            return;

        var player = combatState.Players.FirstOrDefault();
        if (player == null)
            return;

        await KiboCmd.TryAutoPlayRandomNormalCard(player, combatState);
    }

    public bool ShouldPreventKiboAutoPlay(CardModel card)
    {
        if (!card.HasModKeyword(KiboKeywords.UltimateKeyword))
            return false;

        var kiboType = KiboPileManager.GetKiboType(card);
        return kiboType != null && KiboUltimateCooldownState.IsCoolingDown(card.Owner, kiboType);
    }

    /// <summary>
    /// 奇波牌结算后的牌堆路由。
    /// 标准行为是将牌送回活跃牌堆（循环使用）。
    /// 但如果牌所属的奇波在牌结算期间已被换下（例如绝技牌内调用了 SummonRandom），
    /// 则改送回后备牌堆，跟随奇波。
    /// </summary>
    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(
        CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        if (card.HasModKeyword(KiboKeywords.PileMemberKeyword))
        {
            if (pileType != PileType.Discard)
                return (pileType, position);

            var kiboType = KiboPileManager.GetKiboType(card);
            if (kiboType != null)
            {
                // 牌所属的奇波已被换下 → 送回后备牌堆
                if (!KiboPileManager.IsKiboTypeActive(card.Owner, kiboType))
                    return (KiboPileManager.GetStorageCombatPileType(), position);
            }
            // 正常情况：送回活跃堆
            return (KiboPileManager.GetActivePileType(), position);
        }

        return (pileType, position);
    }

    /// <summary>
    /// 奇波牌自动打出并完成路由后触发。
    /// 事后修正：如果牌所属的奇波在结算期间被换下（例如 HiddenInRainbow 的 OnPlay
    /// 内调用了 SummonRandom），则牌在路由阶段已被送回活跃堆（因为路由决定早于 OnPlay），
    /// 此时需将牌从活跃堆搬运到后备堆。
    /// </summary>
    public async Task AfterKiboCardAutoPlayed(CardModel card)
    {
        var kiboType = KiboPileManager.GetKiboType(card);
        if (kiboType == null) return;

        if (card.HasModKeyword(KiboKeywords.UltimateKeyword))
            KiboUltimateCooldownState.StartCooldown(card.Owner, kiboType);

        if (card.Pile == null || card.Pile.Type == PileType.Exhaust || !card.Pile.Type.IsCombatPile())
            return;

        // 奇波仍在活跃中（基于追踪器而非牌堆扫描）→ 无需修正
        if (KiboPileManager.IsKiboTypeActive(card.Owner, kiboType))
        {
            var activePile = KiboPileManager.GetActivePile(card.Owner);
            if (activePile != null && card.Pile.Type != activePile.Type)
                await CardPileCmd.Add(card, activePile);
            return;
        }

        // 奇波在 OnPlay 期间被换下，牌却被路由回了活跃堆 → 搬到后备堆
        var combatStorage = KiboPileManager.GetStorageCombatPile(card.Owner);
        if (combatStorage == null || card.Pile.Type == combatStorage.Type) return;

        await CardPileCmd.Add(card, combatStorage);
    }
}
