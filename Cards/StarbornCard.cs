using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards.Pile;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;
using STS2_Starborn.Hooks;
using STS2_Starborn.Runs;

namespace STS2_Starborn.Cards;

public abstract class StarbornCard(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true
) : ModCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    internal int PrimaryStacks =>
        !IsCanonical && Owner?.Creature?.CombatState != null ? ElementMarkState.GetStacks(Owner, MarkSlot.Primary) : 0;
    internal SealElementType PrimaryElementType =>
        !IsCanonical && Owner?.Creature?.CombatState != null ? ElementMarkState.GetElementType(Owner, MarkSlot.Primary) : SealElementType.None;

    internal int SecondaryStacks =>
        !IsCanonical && Owner?.Creature?.CombatState != null ? ElementMarkState.GetStacks(Owner, MarkSlot.Secondary) : 0;
    internal SealElementType SecondaryElementType =>
        !IsCanonical && Owner?.Creature?.CombatState != null ? ElementMarkState.GetElementType(Owner, MarkSlot.Secondary) : SealElementType.None;

    // ── Any element helpers ────────────────────────────────
    // 当 elementType == SealElementType.Any 时，创建动态解析当前主属性印记的 computed var
    // 预览时显示 Any 图标，战斗时显示实际元素图标

    protected DynamicVar Tuning(int stacks, SealElementType elementType, string name = "Tuning", MarkSlot resolveSlot = MarkSlot.Primary)
    {
        if (elementType != SealElementType.Any)
            return StarbornCardVars.Tuning(stacks, elementType, name);

        var v = new SealElementVar(name, () => stacks, () => SealElementType.Any)
        {
            ResolveFromCurrentMark = true,
            ResolveSlot = resolveSlot,
        };
        v.WithModifyPreview((card, value) =>
        {
            var cs = card.Owner?.Creature?.CombatState;
            return cs != null
                ? SealElementMarkHooks.ModifyTuningConsume(cs, resolveSlot, value)
                : value;
        });
        v.WithTooltip(var =>
        {
            var sev = (SealElementVar)var;
            return StarbornTipFactory.Tuning(sev.ElementType, sev.IntValue);
        });
        return v;
    }

    protected DynamicVar Overload(int stacks, SealElementType elementType, string name = "Overload", MarkSlot resolveSlot = MarkSlot.Primary)
    {
        if (elementType != SealElementType.Any)
            return StarbornCardVars.Overload(stacks, elementType, name);

        var v = new SealElementVar(name, () => stacks, () => SealElementType.Any)
        {
            ResolveFromCurrentMark = true,
            ResolveSlot = resolveSlot,
        };
        v.WithModifyPreview((card, value) =>
        {
            var cs = card.Owner?.Creature?.CombatState;
            return cs != null
                ? SealElementMarkHooks.ModifyOverloadConsume(cs, resolveSlot, value)
                : value;
        });
        v.WithTooltip(var =>
        {
            var sev = (SealElementVar)var;
            return StarbornTipFactory.Overload(sev.ElementType, sev.IntValue);
        });
        return v;
    }

    protected DynamicVar ElementMark(int stacks, SealElementType elementType, string name = "ElementMark", MarkSlot resolveSlot = MarkSlot.Primary)
    {
        if (elementType != SealElementType.Any)
            return StarbornCardVars.ElementMark(stacks, elementType, name);

        var v = new SealElementVar(name, () => stacks, () => SealElementType.Any)
        {
            ResolveFromCurrentMark = true,
            ResolveSlot = resolveSlot,
        };
        v.WithTooltip(var =>
        {
            var sev = (SealElementVar)var;
            return StarbornTipFactory.ElementMark(sev.ElementType, sev.IntValue);
        });
        return v;
    }

    public virtual string? KiboSummonType => null;

    public override async Task AfterCardChangedPiles(
        CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        await base.AfterCardChangedPiles(card, oldPileType, clonedBy);

        if (KiboSummonType is not { } typeId) return;
        if (card.Pile?.Type != PileType.Deck) return;
        if (Owner == null) return;

        var count = Owner.Deck.Cards.OfType<StarbornCard>()
            .Count(c => c.KiboSummonType == typeId);
        if (count > 1) return;

        KiboRunData.Modify(Owner, data =>
        {
            data.OwnedKiboTypeIds.Add(typeId);
            data.ActiveKiboTypeId ??= typeId;
        });
        await KiboPileManager.CreateMasterCards(Owner, typeId);
    }

    public override Task BeforeCardRemoved(CardModel card)
    {
        if (KiboSummonType is not { } typeId) return Task.CompletedTask;
        if (card != this) return Task.CompletedTask;
        if (Owner == null) return Task.CompletedTask;

        var count = Owner.Deck.Cards.OfType<StarbornCard>()
            .Count(c => c.KiboSummonType == typeId);
        if (count > 1) return Task.CompletedTask;

        KiboRunData.Modify(Owner, data =>
        {
            data.OwnedKiboTypeIds.Remove(typeId);
            if (data.ActiveKiboTypeId == typeId)
                data.ActiveKiboTypeId = null;
        });
        KiboPileManager.RemoveMasterCards(Owner, typeId);
        return Task.CompletedTask;
    }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: Const.Paths.CardPortrait(GetType())
        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material（TODO）
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
    );
}
