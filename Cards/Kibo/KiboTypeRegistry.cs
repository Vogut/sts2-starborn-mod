using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2_Starborn.Element;
using STS2_Starborn.UI;

namespace STS2_Starborn.Cards.Kibo;

// ── Attributes ────────────────────────────────────────────

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RegisterKiboAttribute(string kiboTypeStem) : Attribute
{
    /// <summary>Local stem of the Kibo type (e.g. "swift_wolf").</summary>
    public string KiboTypeStem => kiboTypeStem;

    /// <summary>Local stem of the evolution target, if any.</summary>
    public string? EvolvesTo { get; init; }

    public bool IsStarter { get; init; }

    /// <summary>Element type associated with this Kibo. When summoned, sets Primary seal mark to this element.</summary>
    public SealElementType Element { get; init; } = SealElementType.None;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class KiboAbilityOfAttribute(string kiboTypeStem, bool isUltimate = false) : Attribute
{
    /// <summary>Local stem of the Kibo type this ability belongs to.</summary>
    public string KiboTypeStem => kiboTypeStem;

    public bool IsUltimate => isUltimate;
    public int Count { get; init; } = 1;
}

// ── Definition ────────────────────────────────────────────

public sealed record KiboTypeDefinition(
    string TypeId,
    string LocKey,
    string PixelAnimationPath,
    IReadOnlyList<Type> AbilityCardTypes,
    Type RepCardType,
    Type? UltimateCardType = null,
    string? EvolvesTo = null,
    bool IsStarter = false,
    SealElementType Element = SealElementType.None
)
{
    public IEnumerable<IHoverTip> CreatePlayableCardHoverTips()
    {
        foreach (var t in AbilityCardTypes)
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId(t)));

        if (UltimateCardType is { } ultType)
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId(ultType)));
    }

    /// <summary>
    /// 将 RepCard + 所有能力牌打包为紧凑 2 列网格，避免垂直堆叠溢出屏幕。
    /// </summary>
    public CompactCardGridHoverTip CreateCompactCardGridHoverTips()
    {
        var cards = new List<CardModel>
        {
            ModelDb.GetById<CardModel>(ModelDb.GetId(RepCardType)),
        };
        foreach (var tip in CreatePlayableCardHoverTips())
            cards.Add(((CardHoverTip)tip).Card);

        return new CompactCardGridHoverTip(cards);
    }
}

// ── Registry ──────────────────────────────────────────────

public static class KiboTypeRegistry
{
    private static readonly Dictionary<string, KiboTypeDefinition> _definitions = [];

    public static KiboTypeDefinition Get(string stem) => _definitions[stem];

    public static IEnumerable<KiboTypeDefinition> All => _definitions.Values;

    public static KiboTypeDefinition GetByRepCardType(Type repCardType) =>
        _definitions.Values.First(d => d.RepCardType == repCardType);

    public static void Initialize()
    {
        var types = typeof(KiboTypeRegistry).Assembly.GetTypes();

        var kiboDefs = types
            .Select(t => (Type: t, Attr: t.GetCustomAttribute<RegisterKiboAttribute>()))
            .Where(x => x.Attr != null)
            .ToList();

        var abilityDefs = types
            .Select(t => (Type: t, Attr: t.GetCustomAttribute<KiboAbilityOfAttribute>()))
            .Where(x => x.Attr != null)
            .ToLookup(x => x.Attr!.KiboTypeStem);

        foreach (var (repType, attr) in kiboDefs)
        {
            var stem = attr!.KiboTypeStem;
            var group = abilityDefs[stem];
            var abilities = group
                .Where(x => !x.Attr!.IsUltimate)
                .SelectMany(x => Enumerable.Repeat(x.Type, x.Attr!.Count))
                .ToList();

            var ultimate = group
                .Where(x => x.Attr!.IsUltimate)
                .Select(x => (Type?)x.Type)
                .FirstOrDefault();

            _definitions[stem] = new KiboTypeDefinition(
                stem,
                $"kibo_{stem}",
                Const.Paths.KiboPixelAnimation(stem),
                abilities,
                repType,
                ultimate,
                attr.EvolvesTo,
                attr.IsStarter,
                attr.Element);
        }
    }

    /// <summary>
    /// 手动注册（备用），优先级低于 attribute 扫描。同名 TypeId 不会覆盖已有条目。
    /// </summary>
    public static void Register(
        string stem,
        Type repCardType,
        IReadOnlyList<Type> abilityCardTypes,
        Type? ultimateCardType = null,
        string? evolvesTo = null,
        bool isStarter = false)
    {
        if (_definitions.ContainsKey(stem)) return;
        _definitions[stem] = new KiboTypeDefinition(
            stem,
            $"kibo_{stem}",
            Const.Paths.KiboPixelAnimation(stem),
            abilityCardTypes,
            repCardType,
            ultimateCardType,
            evolvesTo,
            isStarter);
    }
}
