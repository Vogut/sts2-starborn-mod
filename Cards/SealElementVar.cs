using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards;

public class SealElementVar : DynamicVar
{
    private readonly Func<int>? _computeValue;
    private readonly Func<SealElementType>? _computeType;
    private readonly SealElementType _staticType;
    private Func<CardModel, int, int>? _modifyPreview;
    private SealElementType _cachedType;

    /// <summary>When true, element type is resolved dynamically from the card's current primary mark.</summary>
    public bool ResolveFromCurrentMark { get; set; }

    /// <summary>Which slot to resolve from when <see cref="ResolveFromCurrentMark"/> is true.</summary>
    public MarkSlot ResolveSlot { get; set; } = MarkSlot.Primary;

    /// <summary>静态构造：卡牌用，元素类型和值固定。</summary>
    public SealElementVar(string name, int value, SealElementType elementType) : base(name, value)
    {
        _staticType = elementType;
        _cachedType = elementType;
    }

    /// <summary>计算构造：Power 用，每次读取时当场计算当前元素类型和值。</summary>
    public SealElementVar(string name, Func<int> computeValue, Func<SealElementType> computeType) : base(name, computeValue())
    {
        _computeValue = computeValue;
        _computeType = computeType;
        _cachedType = computeType();
    }

    public SealElementType ElementType => _cachedType;

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

        if (ResolveFromCurrentMark)
        {
            if (!card.IsCanonical && card is StarbornCard sc)
                _cachedType = ResolveSlot == MarkSlot.Secondary
                    ? sc.SecondaryElementType
                    : sc.PrimaryElementType;
            else
                _cachedType = SealElementType.Any;
        }
        else if (_computeType != null)
        {
            _cachedType = _computeType();
        }
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
