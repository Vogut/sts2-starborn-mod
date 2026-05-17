using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards;

public static class StarbornCardVars
{
    private const string TuningKey = "STS2_STARBORN_TUNING";
    private const string OverloadKey = "STS2_STARBORN_OVERLOAD";
    private const string ElementMarkKey = "STS2_STARBORN_ELEMENT_MARK";

    public static DynamicVar ElementMark(int stacks, SealElementType elementType = SealElementType.None) =>
        new SealElementVar("ElementMark", stacks, elementType).WithSharedTooltip(ElementMarkKey);

    public static DynamicVar Tuning(int stacks, SealElementType elementType = SealElementType.None) =>
        new SealElementVar("Tuning", stacks, elementType).WithSharedTooltip(TuningKey);

    public static DynamicVar Overload(int stacks, SealElementType elementType = SealElementType.None) =>
        new SealElementVar("Overload", stacks, elementType).WithSharedTooltip(OverloadKey);
}
