using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards;

public class SealElementVar : DynamicVar
{
    private readonly Func<int>? _computeValue;
    private readonly Func<SealElementType>? _computeType;
    private readonly SealElementType _staticType;

    /// <summary>静态构造：卡牌用，元素类型和值固定。</summary>
    public SealElementVar(string name, int value, SealElementType elementType) : base(name, value)
    {
        _staticType = elementType;
    }

    /// <summary>计算构造：Power 用，每次读取时当场计算当前元素类型和值。</summary>
    public SealElementVar(string name, Func<int> computeValue, Func<SealElementType> computeType) : base(name, 0)
    {
        _computeValue = computeValue;
        _computeType = computeType;
    }

    public SealElementType ElementType => _computeType?.Invoke() ?? _staticType;

    protected override decimal GetBaseValueForIConvertible()
        => _computeValue?.Invoke() ?? base.GetBaseValueForIConvertible();

    public override string ToString()
        => (_computeValue?.Invoke() ?? (int)BaseValue).ToString();
}
