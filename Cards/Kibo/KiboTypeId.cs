using System;
using System.Collections.Generic;
using STS2RitsuLib.Content;
using STS2RitsuLib.Utils;

namespace STS2_Starborn.Cards.Kibo;

/// <summary>
///     Kibo type identifiers backed by <see cref="DynamicEnumValueRegistry{TEnum}" />.
///     Each Kibo type is defined by a stable local stem string. Qualified IDs (for serialization and cross-mod
///     references) combine the mod id, a category segment, and the local stem.
///     Kibo 类型标识符，基于 <see cref="DynamicEnumValueRegistry{TEnum}" />。
///     每个 Kibo 类型由一个稳定的本地词干字符串定义。限定 ID（用于序列化和跨模组引用）由 mod ID、类别段和本地词干组合而成。
/// </summary>
/// <remarks>
///     <para>
///         Why a static class of string constants instead of a plain C# enum?
///         - Stems produce <b>stable qualified IDs</b> (<c>sts2_starborn_KIBOTYPE_leafox</c>) that survive rename
///         refactors and are safe for cross-save serialization.
///         - The qualified IDs are registered through <see cref="DynamicEnumValueRegistry{TEnum}" />, giving each
///         type a deterministic, collision-free <see cref="KiboTypeIdValue" /> for use with RitsuLib's enum-based
///         extension points.
///         - Attributes still work because <c>const string</c> is a compile-time constant.
///     </para>
///     <para>
///         为什么用字符串常量静态类而不是普通 C# 枚举？
///         - 词干产生<b>稳定的限定 ID</b>（<c>sts2_starborn_KIBOTYPE_leafox</c>），不受重命名重构影响，可安全跨存档序列化。
///         - 限定 ID 通过 <see cref="DynamicEnumValueRegistry{TEnum}" /> 注册，为每个类型提供确定性、无冲突的
///         <see cref="KiboTypeIdValue" />，用于 RitsuLib 基于枚举的扩展点。
///         - 属性仍然可用，因为 <c>const string</c> 是编译期常量。
///     </para>
/// </remarks>
public static class KiboTypeId
{
    // ── Stem constants (stable local identifiers) ────────────────────

    public const string Leafox = "leafox";
    public const string Floratail = "floratail";
    public const string Corovulpe = "corovulpe";
    public const string Flamoo = "flamoo";
    public const string Firebull = "firebull";
    public const string Taurick = "taurick";
    public const string Beaver = "beaver";
    public const string Beaflow = "beaflow";
    public const string MasterBeaver = "master_beaver";
    public const string SwiftWolf = "swift_wolf";
    public const string Moklido = "moklido";
    public const string ArmoredPangolin = "armored_pangolin";
    public const string Downybrinny = "downybrinny";
    public const string MelodiousVine = "melodious_vine";
    public const string JadeFeatherDragon = "jade_feather_dragon";
    public const string SnowWolfPup = "snow_wolf_pup";
    public const string MuroRabbit = "muro_rabbit";
    public const string FlameCrystalArmor = "flame_crystal_armor";
    public const string VineDoll = "vine_doll";
    public const string Bruda = "bruda";
    public const string Cabbird = "cabbird";
    public const string Staray = "staray";
    public const string Gururu = "gururu";
    public const string Phantomfly = "phantomfly";

    /// <summary>All known local stems, in definition order.</summary>
    public static readonly string[] All =
    [
        Leafox,
        Floratail,
        Corovulpe,
        Flamoo,
        Firebull,
        Taurick,
        Beaver,
        Beaflow,
        MasterBeaver,
        SwiftWolf,
        Moklido,
        ArmoredPangolin,
        Downybrinny,
        MelodiousVine,
        JadeFeatherDragon,
        SnowWolfPup,
        MuroRabbit,
        FlameCrystalArmor,
        VineDoll,
        Bruda,
        Cabbird,
        Staray,
        Gururu,
        Phantomfly,
    ];

    private const string CategoryStem = "KIBOTYPE";

    // ── Qualified ID helpers ────────────────────────────────────────

    /// <summary>
    ///     Returns the scoped compound id for <paramref name="stem" />, e.g.
    ///     <c>sts2_starborn_KIBOTYPE_leafox</c>.
    /// </summary>
    public static string GetQualifiedId(string stem)
    {
        return ModContentRegistry.GetCompoundId(Const.ModId, CategoryStem, stem);
    }

    /// <summary>
    ///     Returns the deterministic <see cref="KiboTypeIdValue" /> for <paramref name="stem" />,
    ///     minted by <see cref="DynamicEnumValueRegistry{TEnum}" />.
    /// </summary>
    public static KiboTypeIdValue GetEnumValue(string stem)
    {
        return DynamicEnumValueRegistry<KiboTypeIdValue>.GetValue(GetQualifiedId(stem));
    }

    // ── Serialization helpers ───────────────────────────────────────

    /// <summary>
    ///     Resolves a qualified id (or legacy PascalCase enum name) back into a local stem.
    ///     Returns <c>true</c> if the string could be resolved to a known Kibo type.
    /// </summary>
    public static bool TryParse(string idOrName, out string stem)
    {
        // 1. Try as qualified ID → extract stem
        var suffix = "KIBOTYPE_";
        var idx = idOrName.IndexOf(suffix, StringComparison.OrdinalIgnoreCase);
        if (idx >= 0)
        {
            var candidate = idOrName[(idx + suffix.Length)..];
            if (Array.IndexOf(All, candidate) >= 0)
            {
                stem = candidate;
                return true;
            }
        }

        // 2. Try as a plain stem directly
        if (Array.IndexOf(All, idOrName) >= 0)
        {
            stem = idOrName;
            return true;
        }

        // 3. Legacy: try PascalCase enum name → stem conversion
        var snake = PascalToSnake(idOrName);
        if (Array.IndexOf(All, snake) >= 0)
        {
            stem = snake;
            return true;
        }

        stem = string.Empty;
        return false;
    }

    /// <summary>
    ///     Tries to resolve a qualified id (or legacy name) into a <see cref="KiboTypeIdValue" />.
    /// </summary>
    public static bool TryGetEnumValue(string idOrName, out KiboTypeIdValue value)
    {
        if (TryParse(idOrName, out var stem))
        {
            value = GetEnumValue(stem);
            return true;
        }

        value = default;
        return false;
    }

    // ── Display helpers ─────────────────────────────────────────────

    /// <summary>
    ///     Converts a snake_case stem to PascalCase for file paths / display names.
    ///     e.g. "swift_wolf" → "SwiftWolf", "leafox" → "Leafox".
    /// </summary>
    public static string StemToPascalCase(string stem)
    {
        var sb = new System.Text.StringBuilder(stem.Length);
        var capitalize = true;
        foreach (var c in stem)
        {
            if (c == '_')
            {
                capitalize = true;
                continue;
            }

            sb.Append(capitalize ? char.ToUpperInvariant(c) : c);
            capitalize = false;
        }

        return sb.ToString();
    }

    private static string PascalToSnake(string pascal)
    {
        var sb = new System.Text.StringBuilder();
        for (var i = 0; i < pascal.Length; i++)
        {
            if (i > 0 && char.IsUpper(pascal[i]))
                sb.Append('_');
            sb.Append(char.ToLowerInvariant(pascal[i]));
        }

        return sb.ToString();
    }
}

/// <summary>
///     Underlying enum type for <see cref="DynamicEnumValueRegistry{TEnum}" />-backed Kibo type values.
///     Do not use directly — prefer <see cref="KiboTypeId" /> stems for identification and
///     <see cref="KiboTypeId.GetEnumValue" /> when an enum value is needed.
/// </summary>
public enum KiboTypeIdValue
{
}
