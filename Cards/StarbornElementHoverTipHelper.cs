using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards;

internal static class StarbornElementHoverTipHelper
{
    internal static IHoverTip[] BuildElementMarkEffectTips(
        CardModel card,
        IEnumerable<IHoverTip> existingTips)
    {
        if (card is not StarbornCard)
            return [];

        var elementVars = card.DynamicVars.Values
            .OfType<SealElementVar>()
            .ToArray();
        if (elementVars.Length == 0)
            return [];

        var existingTuning = ElementSet(elementVars, SealElementVarKind.Tuning);
        var existingOverload = ElementSet(elementVars, SealElementVarKind.Overload);
        var markElements = ElementSet(elementVars, SealElementVarKind.ElementMark);
        var referencedElements = ReferencedElementSet(elementVars);
        if (referencedElements.Count == 0)
            return [];

        var tips = new List<IHoverTip>();
        foreach (var elementType in markElements)
        {
            var element = StarbornElement.For(elementType);
            if (!existingTuning.Contains(elementType))
                tips.Add(StarbornTipFactory.Tuning(elementType, element.TuningConsume));
            if (!existingOverload.Contains(elementType))
                tips.Add(StarbornTipFactory.Overload(elementType, element.OverloadConsume));
        }

        var existingPowerTypes = existingTips
            .Select(t => t.CanonicalModel)
            .OfType<PowerModel>()
            .Select(p => p.GetType())
            .ToHashSet();
        foreach (var elementType in referencedElements)
        {
            foreach (var power in StarbornElement.For(elementType).ElementEffectPowers)
            {
                if (existingPowerTypes.Add(power.GetType()))
                    tips.Add(HoverTipFactory.FromPower(power));
            }
        }

        return tips.ToArray();
    }

    private static HashSet<SealElementType> ReferencedElementSet(IEnumerable<SealElementVar> elementVars)
    {
        return elementVars
            .Where(v => v.Kind is SealElementVarKind.ElementMark
                or SealElementVarKind.Tuning
                or SealElementVarKind.Overload)
            .Where(v => IsConcreteElement(v.ElementType))
            .Select(v => v.ElementType)
            .ToHashSet();
    }

    private static HashSet<SealElementType> ElementSet(
        IEnumerable<SealElementVar> elementVars,
        SealElementVarKind kind)
    {
        return elementVars
            .Where(v => v.Kind == kind && IsConcreteElement(v.ElementType))
            .Select(v => v.ElementType)
            .ToHashSet();
    }

    private static bool IsConcreteElement(SealElementType elementType) =>
        elementType is not SealElementType.None and not SealElementType.Any;
}
