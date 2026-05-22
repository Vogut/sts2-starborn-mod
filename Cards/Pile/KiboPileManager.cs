using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.CardPiles;
using STS2RitsuLib.Content;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
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
            Style = ModCardPileUiStyle.Headless,
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
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        // RepCard
        var repCanonical = ModelDb.GetById<CardModel>(ModelDb.GetId(def.RepCardType));
        var repCard = combatState.CreateCard(repCanonical, player);
        repCard.AddModKeyword(KiboKeywords.PileMemberKeywordId);
        repCard.AddModKeyword(keyword);
        await CardPileCmd.Add(repCard, storage);

        // Ability cards
        foreach (var cardType in new[] { def.CardType1, def.CardType2 })
        {
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
            var card = combatState.CreateCard(canonical, player);
            card.AddModKeyword(KiboKeywords.PileMemberKeywordId);
            card.AddModKeyword(keyword);
            await CardPileCmd.Add(card, storage);
        }
    }

    // ── Combat lifecycle ────────────────────────────────────

    public static async Task InitializeForCombat(Player player)
    {
        var data = KiboRunData.Get(player);
        if (data?.OwnedKiboTypeIds == null) return;

        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var masterStorage = GetStoragePile(player);
        var combatStorage = GetStorageCombatPile(player);
        var activePile = GetActivePile(player);
        if (masterStorage == null || combatStorage == null || activePile == null) return;

        // Ensure masters exist for all owned types
        foreach (var typeIdStr in data.OwnedKiboTypeIds)
        {
            if (!Enum.TryParse<KiboTypeId>(typeIdStr, out var typeId)) continue;
            await CreateMasterCards(player, typeId);
        }

        // Clone all ability cards (not RepCards) to combat storage
        foreach (var card in masterStorage.Cards.ToList())
        {
            if (IsRepCardType(card.GetType()))
                continue;

            var clone = combatState.CloneCard(card);
            clone.DeckVersion = card;
            clone.AddModKeyword(KiboKeywords.PileMemberKeywordId);
            await CardPileCmd.Add(clone, combatStorage);
        }

        // Move active type to visible pile
        if (data.ActiveKiboTypeId != null
            && Enum.TryParse<KiboTypeId>(data.ActiveKiboTypeId, out var activeId))
        {
            var keyword = KiboKeywords.TypeKeyword(activeId);
            foreach (var card in combatStorage.Cards
                         .Where(c => c.HasModKeyword(keyword) && !IsRepCardType(c.GetType()))
                         .ToList())
                await CardPileCmd.Add(card, activePile);
        }

        ActiveKiboChanged?.Invoke();
    }

    // ── Switching ───────────────────────────────────────────

    public static async Task SwitchToType(Player player, KiboTypeId typeId)
    {
        var activePile = GetActivePile(player);
        var combatStorage = GetStorageCombatPile(player);
        if (activePile == null || combatStorage == null) return;

        var keyword = KiboKeywords.TypeKeyword(typeId);

        // Acquired mid-combat: clone masters into combat
        if (!combatStorage.Cards.Any(c => c.HasModKeyword(keyword)))
            await CloneTypeIntoCombat(player, typeId);

        // Current active → storage
        foreach (var card in activePile.Cards.ToList())
            await CardPileCmd.Add(card, combatStorage);

        // Target type → active
        foreach (var card in combatStorage.Cards
                     .Where(c => c.HasModKeyword(keyword) && !IsRepCardType(c.GetType()))
                     .ToList())
            await CardPileCmd.Add(card, activePile);

        ActiveKiboChanged?.Invoke();
    }

    public static async Task SummonTemporary(Player player, KiboTypeId typeId)
    {
        var combatState = player.Creature.CombatState;
        if (combatState == null) return;

        var combatStorage = GetStorageCombatPile(player);
        var activePile = GetActivePile(player);
        if (combatStorage == null || activePile == null) return;

        var keyword = KiboKeywords.TypeKeyword(typeId);
        if (combatStorage.Cards.Any(c => c.HasModKeyword(keyword)))
            return; // already present

        var def = KiboTypeRegistry.Get(typeId);
        foreach (var cardType in new[] { def.CardType1, def.CardType2 })
        {
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(cardType));
            var card = combatState.CreateCard(canonical, player);
            card.AddModKeyword(KiboKeywords.PileMemberKeywordId);
            card.AddModKeyword(keyword);
            await CardPileCmd.Add(card, combatStorage);
        }

        // Move current active to storage, target to active
        foreach (var card in activePile.Cards.ToList())
            await CardPileCmd.Add(card, combatStorage);

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
                     .Where(c => c.HasModKeyword(keyword) && !IsRepCardType(c.GetType()))
                     .ToList())
        {
            var clone = combatState.CloneCard(card);
            clone.AddModKeyword(KiboKeywords.PileMemberKeywordId);
            await CardPileCmd.Add(clone, combatStorage);
        }
    }

    // ── Helpers ─────────────────────────────────────────────

    private static bool IsRepCardType(Type type)
    {
        return type.BaseType == typeof(KiboCard) &&
               type.Name.EndsWith("RepCard");
    }
}
