using MegaCrit.Sts2.Core.Localization.DynamicVars;
using SmartFormat.Core.Extensions;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Localization.Formatters;

[RegisterSmartFormatter]
public class SealElementIconsFormatter : IFormatter
{
    public string Name { get => "sealElementIcons"; set => _ = value; }
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

        var elementType = formattingInfo.CurrentValue is SealElementVar sev
            ? sev.ElementType
            : SealElementType.None;

        var tag = $"[img=center]res://STS2_Starborn/powers/Elements/{elementType}Icon.png[/img]";
        formattingInfo.Write(count <= 3
            ? string.Concat(Enumerable.Repeat(tag, count))
            : $"{count}{tag}");

        return true;
    }
}
