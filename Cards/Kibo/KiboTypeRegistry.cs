namespace STS2_Starborn.Cards.Kibo;

public sealed record KiboTypeDefinition(
    KiboTypeId TypeId,
    string LocKey,
    string IconPath,
    Type CardType1,
    Type CardType2,
    Type RepCardType
);

public static class KiboTypeRegistry
{
    private static readonly Dictionary<KiboTypeId, KiboTypeDefinition> _definitions = [];

    public static KiboTypeDefinition Get(KiboTypeId id) => _definitions[id];
    public static IEnumerable<KiboTypeDefinition> All => _definitions.Values;

    public static void Initialize()
    {
        Register(new(
            KiboTypeId.FoxSpirit,
            "kibo_fox_spirit",
            Const.Paths.KiboIcon(KiboTypeId.FoxSpirit),
            typeof(FoxSpiritAbility1Card),
            typeof(FoxSpiritAbility2Card),
            typeof(FoxSpiritRepCard)
        ));
    }

    private static void Register(KiboTypeDefinition def)
    {
        _definitions[def.TypeId] = def;
    }
}
