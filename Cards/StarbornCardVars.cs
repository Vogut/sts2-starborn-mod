using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
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
    /// 计算型调谐变量（Power 用）。元素类型和数值在每次读取时当场计算，tooltip 图标惰性解析。
    /// </summary>
    internal static DynamicVar ComputedTuning(Func<int> value, Func<SealElementType> type)
    {
        var v = new SealElementVar("Tuning", value, type);
        v.WithTooltip(var =>
        {
            var sev = (SealElementVar)var;
            var iconPath = Icon(sev.ElementType);
            var title = new LocString("static_hover_tips", $"{TuningKey}.title");
            var description = new LocString("static_hover_tips", $"{TuningKey}.description");
            title.Add(var);
            description.Add(var);
            Texture2D? icon = null;
            if (!string.IsNullOrWhiteSpace(iconPath) && ResourceLoader.Exists(iconPath))
                icon = ResourceLoader.Load<Texture2D>(iconPath);
            return new HoverTip(title, description, icon);
        });
        return v;
    }

    /// <summary>
    /// 计算型超限变量（Power 用）。元素类型和数值在每次读取时当场计算，tooltip 图标惰性解析。
    /// </summary>
    internal static DynamicVar ComputedOverload(Func<int> value, Func<SealElementType> type)
    {
        var v = new SealElementVar("Overload", value, type);
        v.WithTooltip(var =>
        {
            var sev = (SealElementVar)var;
            var iconPath = Icon(sev.ElementType);
            var title = new LocString("static_hover_tips", $"{OverloadKey}.title");
            var description = new LocString("static_hover_tips", $"{OverloadKey}.description");
            title.Add(var);
            description.Add(var);
            Texture2D? icon = null;
            if (!string.IsNullOrWhiteSpace(iconPath) && ResourceLoader.Exists(iconPath))
                icon = ResourceLoader.Load<Texture2D>(iconPath);
            return new HoverTip(title, description, icon);
        });
        return v;
    }
}
