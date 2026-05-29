using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.CardPiles;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Content;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Commands;
using STS2_Starborn.Runs;
using STS2_Starborn.UI;

namespace STS2_Starborn.Cards.Pile;

/// <summary>
/// 奇波牌堆管理器。
/// 管理三套牌堆：
/// - Storage（全局持久）: 存放所有已拥有奇波的"母版"牌（RepCard + 战技牌 + 绝技牌）
/// - StorageCombat（战斗内）: 战斗开始时从 Storage 克隆的副本，战斗中激活/退场均操作此堆
/// - Active（战斗内）: 当前活跃奇波的战技牌所在堆，回合结束时从此堆自动出牌
/// </summary>
public static class KiboPileManager
{
    // 三套牌堆的 ID 后缀
    public const string StorageStem = "kibo_storage";
    public const string StorageCombatStem = "kibo_storage_combat";
    public const string PileStem = "kibo_pile";

    public static string QualifiedStorageId =>
        ModContentRegistry.GetQualifiedCardPileId(Const.ModId, StorageStem);

    public static string QualifiedStorageCombatId =>
        ModContentRegistry.GetQualifiedCardPileId(Const.ModId, StorageCombatStem);

    public static string QualifiedPileId =>
        ModContentRegistry.GetQualifiedCardPileId(Const.ModId, PileStem);

    /// <summary>活跃奇波切换时触发，供 UI 等订阅刷新。</summary>
    public static event Action? ActiveKiboChanged;

    /// <summary>
    /// 注册三套奇波牌堆到 RitsuLib 牌堆注册表。在 Entry.cs 初始化阶段调用。
    /// </summary>
    public static void RegisterPiles()
    {
        var registry = ModCardPileRegistry.For(Const.ModId);

        registry.RegisterOwned(StorageStem, new ModCardPileSpec
        {
            Scope = ModCardPileScope.RunPersistent,
            Style = ModCardPileUiStyle.TopBarDeck,
            IconPath = Const.Paths.KiboCollectionPileIcon,
            OnOpen = ctx =>
            {
                var viewer = new KiboStorageViewer();
                viewer.Setup(ctx.Pile, ctx.Player);
                ctx.OpenCapstoneScreen(viewer);
            },
        });

        registry.RegisterOwned(StorageCombatStem, new ModCardPileSpec
        {
            Scope = ModCardPileScope.CombatOnly,
            //Style = ModCardPileUiStyle.Headless,

            Style = ModCardPileUiStyle.BottomRight,
            Anchor = new ModCardPileAnchor(ModCardPileAnchorKind.BottomRightPrimary,
                Offset: new Vector2(0, -80f)),
            IconPath = Const.Paths.KiboPileIcon,
        });

        registry.RegisterOwned(PileStem, new ModCardPileSpec
        {
            Scope = ModCardPileScope.CombatOnly,
            Style = ModCardPileUiStyle.BottomLeft,
            Anchor = new ModCardPileAnchor(ModCardPileAnchorKind.BottomLeftPrimary,
                Offset: new Vector2(0, -80f)),
            IconPath = Const.Paths.KiboPileIcon,
        });
    }

    // ── 牌堆访问器 ──────────────────────────────────────

    private static PileType GetPileType(string qualifiedId) =>
        ModCardPileRegistry.GetPileType(qualifiedId);

    public static PileType GetStoragePileType() => GetPileType(QualifiedStorageId);
    public static PileType GetStorageCombatPileType() => GetPileType(QualifiedStorageCombatId);
    public static PileType GetActivePileType() => GetPileType(QualifiedPileId);

    /// <summary>获取全局持久的母版牌堆（全局始终存在）。</summary>
    public static CardPile? GetStoragePile(Player player) =>
        GetStoragePileType().GetPile(player);

    /// <summary>获取战斗内后备牌堆。不在战斗中返回 null。</summary>
    public static CardPile? GetStorageCombatPile(Player player)
    {
        if (player.Creature.CombatState == null) return null;
        return GetStorageCombatPileType().GetPile(player);
    }

    /// <summary>获取战斗内活跃牌堆。不在战斗中返回 null。</summary>
    public static CardPile? GetActivePile(Player player)
    {
        if (player.Creature.CombatState == null) return null;
        return GetActivePileType().GetPile(player);
    }

    // ── 母版牌创建 ────────────────────────────────

    /// <summary>
    /// 在 Storage 牌堆中创建指定奇波类型的全套母版牌（RepCard + 能力牌 + 绝技牌）。
    /// 如果该类型已存在则跳过，保证幂等。
    /// </summary>
    public static async Task CreateMasterCards(Player player, KiboTypeId typeId)
    {
        var storage = GetStoragePile(player);
        if (storage == null) return;

        var keyword = KiboKeywords.TypeKeywordValue(typeId);
        if (storage.Cards.Any(c => c.HasModKeyword(keyword)))
            return; // 已创建，幂等跳过

        var def = KiboTypeRegistry.Get(typeId);

        // 代表牌 RepCard —— 标识奇波类型，不参与战斗出牌
        var repCanonical = ModelDb.GetById<CardModel>(ModelDb.GetId(def.RepCardType));
        var repCard = player.RunState.CreateCard(repCanonical, player);
        repCard.AddModKeyword(KiboKeywords.PileMemberKeyword);
        repCard.AddModKeyword(keyword);
        var repResult = await CardPileCmd.Add(repCard, storage);
        CardCmd.PreviewCardPileAdd(repResult);

        // 能力牌（普通技能/攻击）
        foreach (var cardType in def.AbilityCardTypes)
        {
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
            var card = player.RunState.CreateCard(canonical, player);
            card.AddModKeyword(KiboKeywords.PileMemberKeyword);
            card.AddModKeyword(keyword);
            var abilityResult = await CardPileCmd.Add(card, storage);
            CardCmd.PreviewCardPileAdd(abilityResult);
        }

        // 绝技牌（如有）
        if (def.UltimateCardType is { } ultimateType)
        {
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(ultimateType));
            var card = player.RunState.CreateCard(canonical, player);
            card.AddModKeyword(KiboKeywords.PileMemberKeyword);
            card.AddModKeyword(keyword);
            var ultimateResult = await CardPileCmd.Add(card, storage);
            CardCmd.PreviewCardPileAdd(ultimateResult);
        }
    }

    // ── 母版牌移除 ──────────────────────────────────

    /// <summary>从 Storage 牌堆中移除指定奇波类型的全套母版牌。</summary>
    public static void RemoveMasterCards(Player player, KiboTypeId typeId)
    {
        var storage = GetStoragePile(player);
        if (storage == null) return;

        var keyword = KiboKeywords.TypeKeywordValue(typeId);
        foreach (var card in storage.Cards
                     .Where(c => c.HasModKeyword(keyword))
                     .ToList())
            card.RemoveFromState();
    }

    // ── 战斗生命周期 ────────────────────────────────────

    /// <summary>
    /// 战斗开始时，将 Storage 中的所有母版牌克隆到 StorageCombat。
    /// 后续战斗中 activate/deactivate 均在 StorageCombat 和 Active 之间搬运，母版不受影响。
    /// </summary>
    public static async Task InitializeForCombat(Player player)
    {
        var data = KiboRunData.Get(player);
        if (data == null || data.OwnedKiboTypeIds.Count == 0) return;

        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var masterStorage = GetStoragePile(player);
        var combatStorage = GetStorageCombatPile(player);
        if (masterStorage == null || combatStorage == null) return;

        // 把母版牌全部克隆到战斗后备堆
        foreach (var card in masterStorage.Cards.ToList())
        {
            var clone = combatState.CloneCard(card);
            clone.DeckVersion = card;
            clone.AddModKeyword(KiboKeywords.PileMemberKeyword);
            await CardPileCmd.Add(clone, combatStorage);
        }

    }

    // ── 换下 / 换上 / 切换 ────────────────────────────────

    /// <summary>
    /// 换下：将指定奇波类型的能力牌从活跃堆移回后备堆。
    /// 仅操作牌堆，不触发任何事件或钩子。
    /// </summary>
    public static async Task MoveKiboToStorage(Player player, KiboTypeId typeId)
    {
        var activePile = GetActivePile(player);
        var combatStorage = GetStorageCombatPile(player);
        if (activePile == null || combatStorage == null) return;

        var keyword = KiboKeywords.TypeKeywordValue(typeId);
        foreach (var card in activePile.Cards
                     .Where(c => c.HasModKeyword(keyword) && !IsRepCardType(c.GetType()))
                     .ToList())
            await CardPileCmd.Add(card, combatStorage);
    }

    /// <summary>
    /// 换上：将指定奇波类型的能力牌从后备堆移入活跃堆。
    /// 已在活跃中的类型跳过（幂等）。触发 <see cref="ActiveKiboChanged"/> 事件。
    /// 如果后备堆中不存在该类型，则自动从母版克隆或创建。
    /// </summary>
    public static async Task MoveKiboToActive(Player player, KiboTypeId typeId)
    {
        var activePile = GetActivePile(player);
        var combatStorage = GetStorageCombatPile(player);
        if (activePile == null || combatStorage == null) return;

        var keyword = KiboKeywords.TypeKeywordValue(typeId);

        // 已在活跃中，幂等跳过
        if (activePile.Cards.Any(c => c.HasModKeyword(keyword)))
            return;

        // 后备堆没有该类型 → 从母版克隆或全新创建
        if (!combatStorage.Cards.Any(c => c.HasModKeyword(keyword)))
        {
            var masterStorage = GetStoragePile(player);
            if (masterStorage != null && masterStorage.Cards.Any(c => c.HasModKeyword(keyword)))
                await CreateTypeInCombat(player, typeId);
        }

        // 能力牌从后备堆移到活跃堆（RepCard 留在后备堆不动）
        foreach (var card in combatStorage.Cards
                     .Where(c => c.HasModKeyword(keyword) && !IsRepCardType(c.GetType()))
                     .ToList())
            await CardPileCmd.Add(card, activePile);

        ActiveKiboChanged?.Invoke();
    }

    /// <summary>
    /// 切换奇波：换下旧类型 + 换上新类型。Dismiss + Summon 的组合。
    /// </summary>
    public static async Task ActivateType(Player player, KiboTypeId typeId)
    {
        var fromType = GetActiveKiboType(player);
        if (fromType != null && fromType != typeId)
            await MoveKiboToStorage(player, fromType.Value);
        await MoveKiboToActive(player, typeId);
    }

    /// <summary>从母版 Storage 克隆指定类型到战斗后备堆。</summary>
    private static async Task CloneTypeIntoCombat(Player player, KiboTypeId typeId)
    {
        var masterStorage = GetStoragePile(player);
        var combatStorage = GetStorageCombatPile(player);
        if (masterStorage == null || combatStorage == null) return;

        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var keyword = KiboKeywords.TypeKeywordValue(typeId);
        foreach (var card in masterStorage.Cards
                     .Where(c => c.HasModKeyword(keyword))
                     .ToList())
        {
            var clone = combatState.CloneCard(card);
            clone.AddModKeyword(KiboKeywords.PileMemberKeyword);
            await CardPileCmd.Add(clone, combatStorage);
        }
    }

    /// <summary>在战斗后备堆中全新创建指定类型的牌（不走母版，直接从 ModelDb）。</summary>
    private static async Task CreateTypeInCombat(Player player, KiboTypeId typeId)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;
        var combatStorage = GetStorageCombatPile(player);
        if (combatStorage == null) return;

        var def = KiboTypeRegistry.Get(typeId);
        var keyword = KiboKeywords.TypeKeywordValue(typeId);
        var cardTypes = new List<Type>(def.AbilityCardTypes.Count + 2) { def.RepCardType };
        cardTypes.AddRange(def.AbilityCardTypes);
        if (def.UltimateCardType is { } ultimateType)
            cardTypes.Add(ultimateType);

        foreach (var cardType in cardTypes)
        {
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
            var card = combatState.CreateCard(canonical, player);
            card.AddModKeyword(KiboKeywords.PileMemberKeyword);
            card.AddModKeyword(keyword);
            await CardPileCmd.Add(card, combatStorage);
        }
    }

    // ── 辅助方法 ─────────────────────────────────────────────

    /// <summary>
    /// 返回牌堆中存在的所有奇波类型。通过扫描 RepCard 来判定（每种奇波有且仅有一张 RepCard）。
    /// RepCard 不会离开后备堆，因此无论是 combat storage 还是 active pile 都能正确检测。
    /// </summary>
    public static HashSet<KiboTypeId> GetKiboTypesInPile(CardPile pile)
    {
        var types = new HashSet<KiboTypeId>();
        foreach (var card in pile.Cards)
        {
            if (!IsRepCardType(card.GetType())) continue;
            var typeId = GetKiboType(card);
            if (typeId != null) types.Add(typeId.Value);
        }
        return types;
    }

    /// <summary>根据卡牌上的 type keyword 反查其所属的奇波类型。</summary>
    public static KiboTypeId? GetKiboType(CardModel card)
    {
        foreach (KiboTypeId typeId in Enum.GetValues<KiboTypeId>())
        {
            if (card.HasModKeyword(KiboKeywords.TypeKeywordValue(typeId)))
                return typeId;
        }
        return null;
    }

    /// <summary>获取当前活跃的奇波类型。活跃堆为空时返回 null。</summary>
    public static KiboTypeId? GetActiveKiboType(Player player)
    {
        var activePile = GetActivePile(player);
        if (activePile == null || activePile.Cards.Count == 0) return null;

        foreach (KiboTypeId typeId in Enum.GetValues<KiboTypeId>())
        {
            if (activePile.Cards.Any(c => c.HasModKeyword(KiboKeywords.TypeKeywordValue(typeId))))
                return typeId;
        }
        return null;
    }

    /// <summary>判断卡牌类型是否为 RepCard（继承 KiboCard 且类名以 "RepCard" 结尾）。</summary>
    public static bool IsRepCardType(Type type)
    {
        return type.BaseType == typeof(KiboCard) &&
               type.Name.EndsWith("RepCard");
    }
}
