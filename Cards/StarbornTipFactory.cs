using Godot;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards;

public static class StarbornTipFactory
{
    internal const string TuningKey = "STS2_STARBORN_TUNING";
    internal const string OverloadKey = "STS2_STARBORN_OVERLOAD";

    public static HoverTip Tuning(SealElementType elementType, int consume)
        => Build(TuningKey, "Tuning", elementType, consume);

    public static HoverTip Overload(SealElementType elementType, int consume)
        => Build(OverloadKey, "Overload", elementType, consume);

    public static HoverTip ElementMark(SealElementType elementType, int stacks)
        => Build("STS2_STARBORN_ELEMENT_MARK", "ElementMark", elementType, stacks);

    private static HoverTip Build(string key, string varName,
        SealElementType elementType, int consume)
    {
        var iconPath = Const.Paths.ElementIcon(elementType);
        Texture2D? icon = null;
        if (!string.IsNullOrWhiteSpace(iconPath) && ResourceLoader.Exists(iconPath))
            icon = ResourceLoader.Load<Texture2D>(iconPath);

        var dv = new SealElementVar(varName, consume, elementType);
        var title = key == SealElementLocalization.ElementMarkKey
            ? SealElementLocalization.Title(elementType)
            : new LocString("static_hover_tips", $"{BuildElementKey(key, elementType)}.title");
        var desc = key == SealElementLocalization.ElementMarkKey
            ? SealElementLocalization.Description(elementType)
            : new LocString("static_hover_tips", $"{BuildElementKey(key, elementType)}.description");
        title.Add(dv);
        desc.Add(dv);
        return new HoverTip(title, desc, icon);
    }

    private static string BuildElementKey(string key, SealElementType elementType)
    {
        return elementType == SealElementType.Any
            ? key
            : $"{key}_{elementType.ToString().ToUpperInvariant()}";
    }
}
