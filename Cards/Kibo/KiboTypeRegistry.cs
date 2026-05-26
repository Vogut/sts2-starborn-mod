using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace STS2_Starborn.Cards.Kibo;

// ── Attributes ────────────────────────────────────────────

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class RegisterKiboAttribute(KiboTypeId kiboType) : Attribute
{
    public KiboTypeId KiboType => kiboType;
    public object? EvolvesTo { get; init; }
    public bool IsStarter { get; init; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public sealed class KiboAbilityOfAttribute(KiboTypeId kiboType, bool isUltimate = false) : Attribute
{
    public KiboTypeId KiboType => kiboType;
    public bool IsUltimate => isUltimate;
}

// ── Definition ────────────────────────────────────────────

public sealed record KiboTypeDefinition(
    KiboTypeId TypeId,
    string LocKey,
    string PixelAnimationPath,
    IReadOnlyList<Type> AbilityCardTypes,
    Type RepCardType,
    Type? UltimateCardType = null,
    KiboTypeId? EvolvesTo = null,
    bool IsStarter = false
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
}

// ── Registry ──────────────────────────────────────────────

public static class KiboTypeRegistry
{
    private static readonly Dictionary<KiboTypeId, KiboTypeDefinition> _definitions = [];

    public static KiboTypeDefinition Get(KiboTypeId id) => _definitions[id];

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
            .ToLookup(x => x.Attr!.KiboType);

        foreach (var (repType, attr) in kiboDefs)
        {
            var group = abilityDefs[attr!.KiboType]!;
            var abilities = group
                .Where(x => !x.Attr!.IsUltimate)
                .Select(x => x.Type)
                .ToList();

            var ultimate = group
                .Where(x => x.Attr!.IsUltimate)
                .Select(x => (Type?)x.Type)
                .FirstOrDefault();

            _definitions[attr.KiboType] = new KiboTypeDefinition(
                attr.KiboType,
                $"kibo_{Regex.Replace(attr.KiboType.ToString(), "(?<=.)([A-Z])", "_$1").ToLowerInvariant()}",
                Const.Paths.KiboPixelAnimation(attr.KiboType),
                abilities,
                repType,
                ultimate,
                attr.EvolvesTo as KiboTypeId?,
                attr.IsStarter);
        }
    }

    /// <summary>
    /// 手动注册（备用），优先级低于 attribute 扫描。同名 TypeId 不会覆盖已有条目。
    /// </summary>
    public static void Register(
        KiboTypeId id,
        Type repCardType,
        IReadOnlyList<Type> abilityCardTypes,
        Type? ultimateCardType = null,
        KiboTypeId? evolvesTo = null,
        bool isStarter = false)
    {
        if (_definitions.ContainsKey(id)) return;
        _definitions[id] = new KiboTypeDefinition(
            id,
            $"kibo_{Regex.Replace(id.ToString(), "(?<=.)([A-Z])", "_$1").ToLowerInvariant()}",
            Const.Paths.KiboPixelAnimation(id),
            abilityCardTypes,
            repCardType,
            ultimateCardType,
            evolvesTo,
            isStarter);
    }
}
