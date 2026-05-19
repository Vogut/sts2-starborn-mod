using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2_Starborn.Hooks;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards;

public static class StarbornCardVars
{
    private const string TuningKey = "STS2_STARBORN_TUNING";
    private const string OverloadKey = "STS2_STARBORN_OVERLOAD";
    private const string ElementMarkKey = "STS2_STARBORN_ELEMENT_MARK";


    public static DynamicVar ElementMark(int stacks, SealElementType elementType = SealElementType.None) =>
        new SealElementVar("ElementMark", stacks, elementType).WithSharedTooltip(ElementMarkKey, Const.Paths.ElementIcon(elementType));

    public static DynamicVar Tuning(int stacks, SealElementType elementType = SealElementType.None) =>
        new SealElementVar("Tuning", stacks, elementType).WithSharedTooltip(TuningKey, Const.Paths.ElementIcon(elementType));

    public static DynamicVar Overload(int stacks, SealElementType elementType = SealElementType.None) =>
        new SealElementVar("Overload", stacks, elementType).WithSharedTooltip(OverloadKey, Const.Paths.ElementIcon(elementType));

    /// <summary>构建调谐 hover tip，供 Power 的 <c>AdditionalHoverTips</c> 和 <c>ComputedTuning</c> 复用。</summary>
    internal static HoverTip BuildTuningTip(SealElementType elementType, int consume)
    {
        return BuildTip(TuningKey, "Tuning", elementType, consume);
    }

    /// <summary>构建超限 hover tip，供 Power 的 <c>AdditionalHoverTips</c> 和 <c>ComputedOverload</c> 复用。</summary>
    internal static HoverTip BuildOverloadTip(SealElementType elementType, int consume)
    {
        return BuildTip(OverloadKey, "Overload", elementType, consume);
    }

    /// <summary>构建带图标、标题和描述的统一 <see cref="HoverTip"/>，供 Tuning/Overload 复用。</summary>
    private static HoverTip BuildTip(string key, string varName, SealElementType elementType, int consume)
    {
        var iconPath = Const.Paths.ElementIcon(elementType);
        Texture2D? icon = null;
        if (!string.IsNullOrWhiteSpace(iconPath) && ResourceLoader.Exists(iconPath))
            icon = ResourceLoader.Load<Texture2D>(iconPath);

        var dv = new SealElementVar(varName, consume, elementType);
        var title = new LocString("static_hover_tips", $"{key}.title");
        var description = new LocString("static_hover_tips", $"{key}.description");
        title.Add(dv);
        description.Add(dv);
        return new HoverTip(title, description, icon);
    }

    internal static DynamicVar ComputedTuning(Func<int> value, Func<SealElementType> type)
    {
        var v = new SealElementVar("Tuning", value, type);
        v.WithTooltip(var =>
        {
            var sev = (SealElementVar)var;
            return BuildTuningTip(sev.ElementType, (int)sev.IntValue);
        });
        return v;
    }

    internal static DynamicVar ComputedOverload(Func<int> value, Func<SealElementType> type)
    {
        var v = new SealElementVar("Overload", value, type);
        v.WithTooltip(var =>
        {
            var sev = (SealElementVar)var;
            return BuildOverloadTip(sev.ElementType, (int)sev.IntValue);
        });
        return v;
    }

    /// <summary>供卡面使用的 Tuning 计算 var，自动走 Modify 管线。</summary>
    internal static DynamicVar ComputedCardTuning(
        Func<SealElementMarkPower?> getMark, int baseConsume, SealElementType elementType)
    {
        return ComputedTuning(
            () =>
            {
                var mark = getMark();
                return mark?.CombatState != null
                    ? SealElementMarkHooks.ModifyTuningConsume(mark.CombatState, mark, baseConsume)
                    : baseConsume;
            },
            () => elementType);
    }

    /// <summary>供卡面使用的 Overload 计算 var，自动走 Modify 管线。</summary>
    internal static DynamicVar ComputedCardOverload(
        Func<SealElementMarkPower?> getMark, int baseConsume, SealElementType elementType)
    {
        return ComputedOverload(
            () =>
            {
                var mark = getMark();
                return mark?.CombatState != null
                    ? SealElementMarkHooks.ModifyOverloadConsume(mark.CombatState, mark, baseConsume)
                    : baseConsume;
            },
            () => elementType);
    }
}
