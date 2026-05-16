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

    /// <summary>
    /// 创建"调谐"变量，悬停时展示调谐 tooltip（≥3层触发）。
    /// </summary>
    public static DynamicVar Tuning(int stacks) =>
        ModCardVars.Int("Tuning", stacks).WithSharedTooltip(TuningKey);

    /// <summary>
    /// 创建"超限"变量，悬停时展示超限 tooltip（满层触发）。
    /// </summary>
    public static DynamicVar Overload(int stacks) =>
        ModCardVars.Int("Overload", stacks).WithSharedTooltip(OverloadKey);
}
