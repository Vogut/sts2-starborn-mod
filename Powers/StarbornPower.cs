using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Powers;

/// <summary>
/// Starborn MOD 中所有能力的通用基类，提供统一的继承根节点。
/// </summary>
public abstract class StarbornPower : ModPowerTemplate
{
    /// <summary>
    /// 用于 SmartFormat 注入的 <c>elementPrefix</c> 变量值。返回元素类型名（如 "Fire"），
    /// 配合 <c>{elementPrefix:elementIcon(1)}</c> 渲染固定元素图标。
    /// </summary>
    public virtual string? elementPrefix => null;
}
