using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards;

public class SealElementVar(string name, int value, SealElementType elementType) : DynamicVar(name, value)
{
    public SealElementType ElementType { get; } = elementType;
}
