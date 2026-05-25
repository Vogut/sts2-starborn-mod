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

public static class KiboPileManager
{
    public const string StorageStem = "kibo_storage";
    public const string StorageCombatStem = "kibo_storage_combat";
    public const string PileStem = "kibo_pile";

    public static string QualifiedStorageId =>
        ModContentRegistry.GetQualifiedCardPileId(Const.ModId, StorageStem);

    public static string QualifiedStorageCombatId =>
        ModContentRegistry.GetQualifiedCardPileId(Const.ModId, StorageCombatStem);

    public static string QualifiedPileId =>
        ModContentRegistry.GetQualifiedCardPileId(Const.ModId, PileStem);

    public static event Action? ActiveKiboChanged;

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

    // ── Pile accessors ──────────────────────────────────────

    private static PileType GetPileType(string qualifiedId) =>
        ModCardPileRegistry.GetPileType(qualifiedId);

    public static PileType GetStoragePileType() => GetPileType(QualifiedStorageId);
    public static PileType GetStorageCombatPileType() => GetPileType(QualifiedStorageCombatId);
    public static PileType GetActivePileType() => GetPileType(QualifiedPileId);

    public static CardPile? GetStoragePile(Player player) =>
        GetStoragePileType().GetPile(player);

    public static CardPile? GetStorageCombatPile(Player player)
    {
        if (player.Creature.CombatState == null) return null;
        return GetStorageCombatPileType().GetPile(player);
    }

    public static CardPile? GetActivePile(Player player)
    {
        if (player.Creature.CombatState == null) return null;
        return GetActivePileType().GetPile(player);
    }

    // ── Master card creation ────────────────────────────────

    public static async Task CreateMasterCards(Player player, KiboTypeId typeId)
    {
        var storage = GetStoragePile(player);
        if (storage == null) return;

        var keyword = KiboKeywords.TypeKeyword(typeId);
        if (storage.Cards.Any(c => c.HasModKeyword(keyword)))
            return; // already created

        var def = KiboTypeRegistry.Get(typeId);

        // RepCard
        var repCanonical = ModelDb.GetById<CardModel>(ModelDb.GetId(def.RepCardType));
        var repCard = player.RunState.CreateCard(repCanonical, player);
        repCard.AddModKeyword(KiboKeywords.PileMemberKeywordId);
        repCard.AddModKeyword(keyword);
        await CardPileCmd.Add(repCard, storage);

        // Ability cards
        foreach (var cardType in new[] { def.CardType1, def.CardType2 })
        {
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
            var card = player.RunState.CreateCard(canonical, player);
            card.AddModKeyword(KiboKeywords.PileMemberKeywordId);
            card.AddModKeyword(keyword);
            await CardPileCmd.Add(card, storage);
        }
    }

    // ── Master card removal ──────────────────────────────────

    public static void RemoveMasterCards(Player player, KiboTypeId typeId)
    {
        var storage = GetStoragePile(player);
        if (storage == null) return;

        var keyword = KiboKeywords.TypeKeyword(typeId);
        foreach (var card in storage.Cards
                     .Where(c => c.HasModKeyword(keyword))
                     .ToList())
            card.RemoveFromState();
    }

    // ── Combat lifecycle ────────────────────────────────────

    public static async Task InitializeForCombat(Player player)
    {
        var data = KiboRunData.Get(player);
        if (data == null || data.OwnedKiboTypeIds.Count == 0) return;

        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var masterStorage = GetStoragePile(player);
        var combatStorage = GetStorageCombatPile(player);
        if (masterStorage == null || combatStorage == null) return;

        // Clone all cards (including RepCards) from master to combat storage
        foreach (var card in masterStorage.Cards.ToList())
        {
            var clone = combatState.CloneCard(card);
            clone.DeckVersion = card;
            clone.AddModKeyword(KiboKeywords.PileMemberKeywordId);
            await CardPileCmd.Add(clone, combatStorage);
        }

    }

    // ── Activate ────────────────────────────────────────────

    public static async Task ActivateType(Player player, KiboTypeId typeId)
    {
        var activePile = GetActivePile(player);
        var combatStorage = GetStorageCombatPile(player);
        if (activePile == null || combatStorage == null) return;

        var keyword = KiboKeywords.TypeKeyword(typeId);

        // Already active
        if (activePile.Cards.Any(c => c.HasModKeyword(keyword)))
            return;

        // Not in combatStorage → create (clone from master, or fresh)
        if (!combatStorage.Cards.Any(c => c.HasModKeyword(keyword)))
        {
            var masterStorage = GetStoragePile(player);
            if (masterStorage != null && masterStorage.Cards.Any(c => c.HasModKeyword(keyword)))
                await CloneTypeIntoCombat(player, typeId);
            else
                await CreateTypeInCombat(player, typeId);
        }

        // Current active → combatStorage
        foreach (var card in activePile.Cards.ToList())
            await CardPileCmd.Add(card, combatStorage);

        // Target ability cards → activePile (RepCard stays in combatStorage)
        foreach (var card in combatStorage.Cards
                     .Where(c => c.HasModKeyword(keyword) && !IsRepCardType(c.GetType()))
                     .ToList())
            await CardPileCmd.Add(card, activePile);

        ActiveKiboChanged?.Invoke();
    }

    private static async Task CloneTypeIntoCombat(Player player, KiboTypeId typeId)
    {
        var masterStorage = GetStoragePile(player);
        var combatStorage = GetStorageCombatPile(player);
        if (masterStorage == null || combatStorage == null) return;

        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var keyword = KiboKeywords.TypeKeyword(typeId);
        foreach (var card in masterStorage.Cards
                     .Where(c => c.HasModKeyword(keyword))
                     .ToList())
        {
            var clone = combatState.CloneCard(card);
            clone.AddModKeyword(KiboKeywords.PileMemberKeywordId);
            await CardPileCmd.Add(clone, combatStorage);
        }
    }

    private static async Task CreateTypeInCombat(Player player, KiboTypeId typeId)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;
        var combatStorage = GetStorageCombatPile(player);
        if (combatStorage == null) return;

        var def = KiboTypeRegistry.Get(typeId);
        var keyword = KiboKeywords.TypeKeyword(typeId);
        foreach (var cardType in new[] { def.RepCardType, def.CardType1, def.CardType2 })
        {
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
            var card = combatState.CreateCard(canonical, player);
            card.AddModKeyword(KiboKeywords.PileMemberKeywordId);
            card.AddModKeyword(keyword);
            await CardPileCmd.Add(card, combatStorage);
        }
    }

    // ── Helpers ─────────────────────────────────────────────

    /// <summary>
    /// 返回 <paramref name="pile"/> 中存在的所有奇波类型（仅检测非 RepCard 的能力牌）。
    /// </summary>
    public static HashSet<KiboTypeId> GetKiboTypesInPile(CardPile pile)
    {
        var types = new HashSet<KiboTypeId>();
        foreach (KiboTypeId typeId in Enum.GetValues<KiboTypeId>())
        {
            var keyword = KiboKeywords.TypeKeyword(typeId);
            if (pile.Cards.Any(c =>
                    c.HasModKeyword(keyword) &&
                    !IsRepCardType(c.GetType())))
            {
                types.Add(typeId);
            }
        }
        return types;
    }

    public static KiboTypeId? GetKiboType(CardModel card)
    {
        foreach (KiboTypeId typeId in Enum.GetValues<KiboTypeId>())
        {
            if (card.HasModKeyword(KiboKeywords.TypeKeyword(typeId)))
                return typeId;
        }
        return null;
    }

    public static KiboTypeId? GetActiveKiboType(Player player)
    {
        var activePile = GetActivePile(player);
        if (activePile == null || activePile.Cards.Count == 0) return null;

        foreach (KiboTypeId typeId in Enum.GetValues<KiboTypeId>())
        {
            if (activePile.Cards.Any(c => c.HasModKeyword(KiboKeywords.TypeKeyword(typeId))))
                return typeId;
        }
        return null;
    }

    /// <summary>
    /// 从活跃牌堆中随机选一张普通能力牌自动打出。返回是否成功打出。
    /// </summary>
    public static async Task<bool> TryAutoPlayRandomNormalCard(
        Player player, ICombatState combatState)
    {
        var pile = GetActivePile(player);
        if (pile == null) return false;

        var normalCards = pile.Cards
            .Where(c => c.HasModKeyword(KiboKeywords.NormalKeywordId))
            .ToList();
        if (normalCards.Count == 0) return false;

        var card = normalCards[Random.Shared.Next(normalCards.Count)];
        await KiboCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, combatState);
        return true;
    }

    public static bool IsRepCardType(Type type)
    {
        return type.BaseType == typeof(KiboCard) &&
               type.Name.EndsWith("RepCard");
    }
}
