using System;
using System.Collections.Generic;
using SmartFormat.Core.Extensions;
using SmartFormat.Core.Parsing;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Cards;

namespace STS2_Starborn.Localization.Formatters;

/// <summary>
/// SmartFormat formatter for conditionally rendering text based on <see cref="IfCanOverloadVar"/>.
/// Usage: {IfCanOverload:showOverload:超限效果|调谐效果}
/// When primary mark stacks > threshold (≥4), renders the first option; otherwise the second.
/// </summary>
[RegisterSmartFormatter]
public class ShowIfCanOverloadFormatter : IFormatter
{
    public string Name { get => "showOverload"; set => _ = value; }
    public bool CanAutoDetect { get; set; }

    public bool TryEvaluateFormat(IFormattingInfo formattingInfo)
    {
        if (formattingInfo.CurrentValue is not IfCanOverloadVar ifCanOverloadVar)
            return false;

        IList<Format>? options = formattingInfo.Format?.Split('|');
        if (options == null)
        {
            throw new Exception($"Format expression must contain at least 1 option. format={formattingInfo.Format}.");
        }
        if (options.Count > 2)
        {
            throw new Exception($"Format expression cannot contain more than 2 options. num_of_options={options.Count} format={formattingInfo.Format}.");
        }

        Format overloadFormat = options[0];
        Format? tuningFormat = options.Count > 1 ? options[1] : null;

        // PreviewValue: 1 = can overload (>3 stacks), 0 = tuning only (≤3 stacks)
        if (ifCanOverloadVar.PreviewValue > 0)
        {
            formattingInfo.FormatAsChild(overloadFormat, formattingInfo.CurrentValue);
        }
        else if (tuningFormat != null)
        {
            formattingInfo.FormatAsChild(tuningFormat, formattingInfo.CurrentValue);
        }

        return true;
    }
}
