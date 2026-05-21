using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_Starborn.Cards.Kibo;

[RegisterOwnedCardKeyword("kibo")]
[RegisterOwnedCardKeyword("kibo_pile_member", IncludeInCardHoverTip = false)]
[RegisterOwnedCardKeyword("kibo_normal")]
[RegisterOwnedCardKeyword("kibo_ultimate")]
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
}
