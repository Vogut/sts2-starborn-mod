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
[RegisterOwnedCardKeyword("kibo_type_swiftwolf", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_moklido", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_armored_pangolin", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_downybrinny", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_type_melodious_vine", IncludeInCardHoverTip = false)]
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

    public static string TypeKeyword(KiboTypeId typeId) =>
        ModContentRegistry.GetQualifiedKeywordId(Const.ModId,
            $"kibo_type_{PascalToSnake(typeId.ToString())}");

    public static readonly CardKeyword PileMemberKeyword = PileMemberKeywordId.GetModCardKeyword();
    public static readonly CardKeyword NormalKeyword = NormalKeywordId.GetModCardKeyword();
    public static readonly CardKeyword UltimateKeyword = UltimateKeywordId.GetModCardKeyword();
    public static CardKeyword TypeKeywordValue(KiboTypeId typeId) =>
        TypeKeyword(typeId).GetModCardKeyword();

    private static string PascalToSnake(string pascal)
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < pascal.Length; i++)
        {
            if (i > 0 && char.IsUpper(pascal[i]))
                sb.Append('_');
            sb.Append(char.ToLowerInvariant(pascal[i]));
        }
        return sb.ToString();
    }
}
