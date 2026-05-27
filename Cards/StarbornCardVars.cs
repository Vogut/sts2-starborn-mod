using Godot;
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
    private const string ElementMarkKey = "STS2_STARBORN_ELEMENT_MARK";


    public static DynamicVar ElementMark(int stacks, SealElementType elementType = SealElementType.None, string name = "ElementMark") =>
        new SealElementVar(name, stacks, elementType).WithSharedTooltip(ElementMarkKey, Const.Paths.ElementIcon(elementType));

    public static DynamicVar Tuning(int stacks, SealElementType elementType = SealElementType.None, string name = "Tuning") =>
        new SealElementVar(name, stacks, elementType)
            .WithModifyPreview((card, v) =>
            {
                var combatState = card.Owner.Creature.CombatState;
                return combatState != null
                    ? SealElementMarkHooks.ModifyTuningConsume(combatState, MarkSlot.Primary, v)
                    : v;
            })
            .WithSharedTooltip(StarbornTipFactory.TuningKey, Const.Paths.ElementIcon(elementType));

    public static DynamicVar Overload(int stacks, SealElementType elementType = SealElementType.None, string name = "Overload") =>
        new SealElementVar(name, stacks, elementType)
            .WithModifyPreview((card, v) =>
            {
                var combatState = card.Owner.Creature.CombatState;
                return combatState != null
                    ? SealElementMarkHooks.ModifyOverloadConsume(combatState, MarkSlot.Primary, v)
                    : v;
            })
            .WithSharedTooltip(StarbornTipFactory.OverloadKey, Const.Paths.ElementIcon(elementType));

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
}
