using System.Text.RegularExpressions;

namespace STS2_Starborn.Cards.Kibo;

public sealed record KiboTypeDefinition(
    KiboTypeId TypeId,
    string LocKey,
    string PixelAnimationPath,
    Type CardType1,
    Type CardType2,
    Type RepCardType,
    KiboTypeId? EvolvesTo = null
);

public static class KiboTypeRegistry
{
    private static readonly Dictionary<KiboTypeId, KiboTypeDefinition> _definitions = [];

    public static KiboTypeDefinition Get(KiboTypeId id) => _definitions[id];
    public static IEnumerable<KiboTypeDefinition> All => _definitions.Values;

    public static void Initialize()
    {
        Register(KiboTypeId.FoxSpirit,
            typeof(FoxSpiritAbility1Card),
            typeof(FoxSpiritAbility2Card),
            typeof(FoxSpiritRepCard),
            evolvesTo: KiboTypeId.ShadowWolf);
        Register(KiboTypeId.ShadowWolf,
            typeof(ShadowWolfAbility1Card),
            typeof(ShadowWolfAbility2Card),
            typeof(ShadowWolfRepCard),
            evolvesTo: KiboTypeId.ThunderHawk);
        Register(KiboTypeId.ThunderHawk,
            typeof(ThunderHawkAbility1Card),
            typeof(ThunderHawkAbility2Card),
            typeof(ThunderHawkRepCard),
            evolvesTo: null);
    }

    private static void Register(KiboTypeId id, Type cardType1, Type cardType2, Type repCardType,
        KiboTypeId? evolvesTo = null)
    {
        _definitions[id] = new KiboTypeDefinition(
            id,
            $"kibo_{Regex.Replace(id.ToString(), "(?<=.)([A-Z])", "_$1").ToLowerInvariant()}",
            Const.Paths.KiboPixelAnimation(id),
            cardType1, cardType2, repCardType,
            evolvesTo);
    }
}
