using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SmartFormat.Core.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Localization.Formatters;

/// <summary>
/// SmartFormat 格式化器：将 ElementMark 数值渲染为内联元素图标。
/// 用法：{ElementMark:elementMarkIcons()}，渲染后显示对应元素 PNG 图标 x 层数。
/// 非战斗环境回退为 None 图标。
/// </summary>
[RegisterSmartFormatter]
public class ElementMarkIconsFormatter : IFormatter
{
    public string Name { get => "elementMarkIcons"; set => _ = value; }
    public bool CanAutoDetect { get; set; }

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        var count = formattingInfo.CurrentValue switch
        {
            DynamicVar dv => (int)dv.IntValue,
            int i => i,
            decimal d => (int)d,
            _ => -1,
        };
        if (count <= 0) return false;

        var elementType = ResolveElementType();
        var iconPath = $"res://STS2_Starborn/powers/Elements/{elementType}Icon.png";

        var tag = $"[img=center]{iconPath}[/img]";
        formattingInfo.Write(count <= 5
            ? string.Concat(Enumerable.Repeat(tag, count))
            : $"{count}{tag}");

        return true;
    }

    private static SealElementType ResolveElementType()
    {
        // TODO：从战斗中读取印记当前属性动态切换图标。目前仅 None.png 存在，始终返回 None。
        return SealElementType.None;
    }
}
