using MegaCrit.Sts2.Core.Localization;

namespace STS2_Starborn.Element;

internal static class SealElementLocalization
{
    internal const string Table = "static_hover_tips";
    internal const string ElementMarkKey = "STS2_STARBORN_ELEMENT_MARK";

    internal static string EntryKey(SealElementType elementType)
    {
        return elementType == SealElementType.Any
            ? ElementMarkKey
            : $"{ElementMarkKey}_{elementType.ToString().ToUpperInvariant()}";
    }

    internal static LocString Title(SealElementType elementType)
    {
        return new LocString(Table, $"{EntryKey(elementType)}.title");
    }

    internal static LocString Description(SealElementType elementType)
    {
        return new LocString(Table, $"{EntryKey(elementType)}.description");
    }
}
