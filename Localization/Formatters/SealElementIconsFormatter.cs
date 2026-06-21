using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SmartFormat.Core.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards;
using STS2_Starborn.Element;

namespace STS2_Starborn.Localization.Formatters;

[RegisterSmartFormatSource]
public class SealElementLiteralSource : ISource
{
    public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        if (!string.Equals(selectorInfo.SelectorText, "element", StringComparison.OrdinalIgnoreCase))
            return false;

        selectorInfo.Result = SealElementLiteralValue.Instance;
        return true;
    }
}

internal sealed class SealElementLiteralValue : IFormattable
{
    internal static readonly SealElementLiteralValue Instance = new();

    private SealElementLiteralValue()
    {
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        if (!Enum.TryParse<SealElementType>(format, ignoreCase: true, out var elementType))
            return string.Empty;

        return $"[img=center]{Const.Paths.ElementIcon(elementType)}[/img]";
    }

    public override string ToString()
    {
        return string.Empty;
    }
}

[RegisterSmartFormatter]
public class SealElementIconsFormatter : IFormatter
{
    public string Name { get => "elementIcon"; set => _ = value; }
    public bool CanAutoDetect { get; set; }

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        var single = formattingInfo.FormatterOptions == "single";

        if (formattingInfo.CurrentValue is string elementName)
        {
            if (!System.Enum.TryParse<SealElementType>(elementName, ignoreCase: true, out var parsedType))
                return false;
            if (!int.TryParse(formattingInfo.FormatterOptions, out var fixedCount) || fixedCount < 0)
                return false;

            var icon = $"[img=center]{Const.Paths.ElementIcon(parsedType)}[/img]";
            formattingInfo.Write(fixedCount is > 0 and <= 3
                ? string.Concat(Enumerable.Repeat(icon, fixedCount))
                : $"{fixedCount}{icon}");
            return true;
        }

        var count = formattingInfo.CurrentValue switch
        {
            SealElementVar se => (int)se.PreviewValue,
            DynamicVar dv => (int)dv.IntValue,
            int i => i,
            decimal d => (int)d,
            _ => single ? 1 : -1,
        };
        if (!single && count < 0) return false;

        var elementType = formattingInfo.CurrentValue is SealElementVar sev
            ? sev.ElementType
            : SealElementType.None;

        var iconPath = Const.Paths.ElementIcon(elementType);
        var tag = $"[img=center]{iconPath}[/img]";
        if (single)
            formattingInfo.Write(tag);
        else
            formattingInfo.Write(count <= 3 && count > 0
                ? string.Concat(Enumerable.Repeat(tag, count))
                : $"{count}{tag}");

        return true;
    }
}
