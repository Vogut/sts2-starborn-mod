using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;

namespace STS2_Starborn.Cards;

/// <summary>
/// Starborn 模组专用 DynamicVar 工厂，统一管理卡牌变量及其悬停 tooltip 绑定。
/// </summary>
public static class StarbornCardVars
{
    private const string TuningKey = "STS2_STARBORN_TUNING";
    private const string OverloadKey = "STS2_STARBORN_OVERLOAD";
    private const string ElementMarkKey = "STS2_STARBORN_ELEMENT_MARK";
    /// <summary>
    /// "属性印记"变量。
    /// </summary>
    public static DynamicVar ElementMark(int stacks) =>
        ModCardVars.Int("ElementMark", stacks).WithSharedTooltip(ElementMarkKey);
    /// <summary>
    /// "调谐"变量。消耗至多3枚印记触发调谐效果。
    /// </summary>
    public static DynamicVar Tuning(int stacks) =>
        ModCardVars.Int("Tuning", stacks).WithSharedTooltip(TuningKey);

    /// <summary>
    /// "超限"变量。印记达到3层以上时才可使用，消耗至多3枚印记触发超限效果。
    /// </summary>
    public static DynamicVar Overload(int stacks) =>
        ModCardVars.Int("Overload", stacks).WithSharedTooltip(OverloadKey);
}
