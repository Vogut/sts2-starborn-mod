using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards;

public static class StarbornCardVars
{
    private const string TuningKey = "STS2_STARBORN_TUNING";
    private const string OverloadKey = "STS2_STARBORN_OVERLOAD";
    private const string ElementMarkKey = "STS2_STARBORN_ELEMENT_MARK";

    private static string Icon(SealElementType et) =>
        $"res://STS2_Starborn/powers/Elements/{et}Icon.png";

    public static DynamicVar ElementMark(int stacks, SealElementType elementType = SealElementType.None) =>
        new SealElementVar("ElementMark", stacks, elementType).WithSharedTooltip(ElementMarkKey, Icon(elementType));

    public static DynamicVar Tuning(int stacks, SealElementType elementType = SealElementType.None) =>
        new SealElementVar("Tuning", stacks, elementType).WithSharedTooltip(TuningKey, Icon(elementType));

    public static DynamicVar Overload(int stacks, SealElementType elementType = SealElementType.None) =>
        new SealElementVar("Overload", stacks, elementType).WithSharedTooltip(OverloadKey, Icon(elementType));

    /// <summary>
    /// 元素切换时刷新 power 的 {Tuning}/{Overload} DynamicVar 数值、元素图标和 tooltip。
    /// </summary>
    internal static void RefreshPowerVars(DynamicVarSet vars, ElementPower ep, SealElementType et)
    {
        if (vars.TryGetValue("Tuning", out var tv) && tv is SealElementVar tsev)
        {
            tsev.BaseValue = ep.TuningConsume;
            tsev.ElementType = et;
            tsev.WithSharedTooltip(TuningKey, Icon(et));
        }
        if (vars.TryGetValue("Overload", out var ov) && ov is SealElementVar osev)
        {
            osev.BaseValue = ep.OverloadConsume;
            osev.ElementType = et;
            osev.WithSharedTooltip(OverloadKey, Icon(et));
        }
    }
}
