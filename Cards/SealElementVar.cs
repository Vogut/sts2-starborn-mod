using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards;

public class SealElementVar : DynamicVar
{
    private readonly Func<int>? _computeValue;
    private readonly Func<SealElementType>? _computeType;
    private readonly SealElementType _staticType;
    private Func<CardModel, int, int>? _modifyPreview;

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

    /// <summary>注入 Modify 预览函数，对标原版 <c>DamageVar.UpdateCardPreview</c> 调用 <c>Hook.ModifyDamage</c>。</summary>
    public SealElementVar WithModifyPreview(Func<CardModel, int, int> modifyPreview)
    {
        _modifyPreview = modifyPreview;
        return this;
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        decimal num = _computeValue?.Invoke() ?? BaseValue;
        if (_modifyPreview != null && runGlobalHooks)
            num = _modifyPreview(card, (int)num);
        PreviewValue = num;
    }

    protected override decimal GetBaseValueForIConvertible()
    {
        return _computeValue?.Invoke() ?? base.GetBaseValueForIConvertible();
    }

    public override string ToString()
    {
        return (_computeValue?.Invoke() ?? (int)BaseValue).ToString();
    }
}
