using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Scaffolding.Characters;
using STS2_Starborn.Combat;
using STS2_Starborn.Hooks;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards;

public static class StarbornCardVars
{
    public static DynamicVar ElementMark(int stacks, SealElementType elementType = SealElementType.None, string name = "ElementMark") =>
        new SealElementVar(name, stacks, elementType)
            .WithTooltip(var =>
            {
                var sev = (SealElementVar)var;
                return StarbornTipFactory.ElementMark(sev.ElementType, sev.IntValue);
            });

    public static DynamicVar Tuning(int stacks, SealElementType elementType = SealElementType.None, string name = "Tuning") =>
        new SealElementVar(name, stacks, elementType)
            .WithModifyPreview((card, v) =>
            {
                var combatState = card.Owner.Creature.CombatState;
                return combatState != null
                    ? SealElementMarkHooks.ModifyTuningConsume(combatState, MarkSlot.Primary, v)
                    : v;
            })
            .WithTooltip(var =>
            {
                var sev = (SealElementVar)var;
                return StarbornTipFactory.Tuning(sev.ElementType, sev.IntValue);
            });

    public static DynamicVar Overload(int stacks, SealElementType elementType = SealElementType.None, string name = "Overload") =>
        new SealElementVar(name, stacks, elementType)
            .WithModifyPreview((card, v) =>
            {
                var combatState = card.Owner.Creature.CombatState;
                return combatState != null
                    ? SealElementMarkHooks.ModifyOverloadConsume(combatState, MarkSlot.Primary, v)
                    : v;
            })
            .WithTooltip(var =>
            {
                var sev = (SealElementVar)var;
                return StarbornTipFactory.Overload(sev.ElementType, sev.IntValue);
            });

    internal static DynamicVar ComputedTuning(Func<int> value, Func<SealElementType> type, string name = "Tuning")
    {
        var v = new SealElementVar(name, value, type);
        v.WithTooltip(var =>
        {
            var sev = (SealElementVar)var;
            return StarbornTipFactory.Tuning(sev.ElementType, (int)sev.IntValue);
        });
        return v;
    }

    internal static DynamicVar ComputedOverload(Func<int> value, Func<SealElementType> type, string name = "Overload")
    {
        var v = new SealElementVar(name, value, type);
        v.WithTooltip(var =>
        {
            var sev = (SealElementVar)var;
            return StarbornTipFactory.Overload(sev.ElementType, (int)sev.IntValue);
        });
        return v;
    }

    /// <summary>
    /// Creates an <see cref="IfCanOverloadVar"/> — true when primary mark stacks > ThresholdStacks (3).
    /// Use in a card's <c>CanonicalVars</c> along with <c>StarbornCardVars.Overload(...)</c> and
    /// the JSON conditional <c>{IfCanOverload:超限{Overload:elementIcon()}|调谐{Tuning:elementIcon()}}</c>.
    /// </summary>
    public static DynamicVar IfCanOverload(MarkSlot slot = MarkSlot.Primary) => new IfCanOverloadVar(slot);
}

/// <summary>
/// DynamicVar that indicates whether the current primary seal mark has enough stacks
/// to trigger overload instead of tuning (stacks > ThresholdStacks, i.e. ≥ 4).
/// Used in card descriptions with SmartFormat native conditional syntax.
/// Add to a card's CanonicalVars via <c>StarbornCardVars.IfCanOverload()</c>.
/// </summary>
public class IfCanOverloadVar : DynamicVar
{
    public const string DefaultName = "IfCanOverload";
    private readonly MarkSlot _slot;

    public IfCanOverloadVar(MarkSlot slot = MarkSlot.Primary)
        : base(DefaultName, 0)
    {
        _slot = slot;
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        if (!card.IsCanonical && card.Owner?.Creature?.CombatState != null)
        {
            ElementMarkState.TryGetStacks(card.Owner, _slot, out var stacks);
            PreviewValue = stacks > ElementMarkState.ThresholdStacks ? 1 : 0;
        }
        else
        {
            PreviewValue = 0;
        }
    }
}
