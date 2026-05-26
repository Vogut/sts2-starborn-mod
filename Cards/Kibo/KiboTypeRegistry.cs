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
        Register(KiboTypeId.Leafox,
            typeof(LeafoxAbility1Card),
            typeof(LeafoxAbility2Card),
            typeof(LeafoxRepCard),
            evolvesTo: KiboTypeId.Floratail);
        Register(KiboTypeId.Floratail,
            typeof(FloratailAbility1Card),
            typeof(FloratailAbility2Card),
            typeof(FloratailRepCard),
            evolvesTo: KiboTypeId.Corovulpe);
        Register(KiboTypeId.Corovulpe,
            typeof(CorovulpeAbility1Card),
            typeof(CorovulpeAbility2Card),
            typeof(CorovulpeRepCard),
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
