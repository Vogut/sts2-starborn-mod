using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace STS2_Starborn.Cards.Kibo;

[RegisterOwnedCardKeyword("kibo")]
[RegisterOwnedCardKeyword("kibo_pile_member", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_normal", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword("kibo_ultimate", CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]
[RegisterOwnedCardKeyword("kibo_type_leafox", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_floratail", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_corovulpe", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_swift_wolf", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_moklido", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_armored_pangolin", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_downybrinny", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_melodious_vine", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_jade_feather_dragon", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_snow_wolf_pup", IncludeInCardHoverTip = false)]
public class KiboKeywords
{
    public static readonly string KiboKeywordId =
        ModContentRegistry.GetQualifiedKeywordId(Const.ModId, "kibo");

    public static readonly string PileMemberKeywordId =
        ModContentRegistry.GetQualifiedKeywordId(Const.ModId, "kibo_pile_member");

    public static readonly string NormalKeywordId =
        ModContentRegistry.GetQualifiedKeywordId(Const.ModId, "kibo_normal");

    public static readonly string UltimateKeywordId =
        ModContentRegistry.GetQualifiedKeywordId(Const.ModId, "kibo_ultimate");

    /// <summary>
    ///     Returns the qualified keyword id for a Kibo type stem (e.g. "swift_wolf" →
    ///     <c>sts2_starborn_KEYWORD_kibo_type_swift_wolf</c>).
    /// </summary>
    public static string TypeKeyword(string stem) =>
        ModContentRegistry.GetQualifiedKeywordId(Const.ModId, $"kibo_type_{stem}");

    public static readonly CardKeyword PileMemberKeyword = PileMemberKeywordId.GetModCardKeyword();
    public static readonly CardKeyword NormalKeyword = NormalKeywordId.GetModCardKeyword();
    public static readonly CardKeyword UltimateKeyword = UltimateKeywordId.GetModCardKeyword();

    /// <summary>
    ///     Returns the <see cref="CardKeyword" /> for a Kibo type stem.
    /// </summary>
    public static CardKeyword TypeKeywordValue(string stem) =>
        TypeKeyword(stem).GetModCardKeyword();
}
