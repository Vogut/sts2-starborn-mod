using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_Starborn.Kibo;

[RegisterOwnedCardKeyword("kibo")]
[RegisterOwnedCardKeyword("kibo_pile_member", IncludeInCardHoverTip = false)]
public static class KiboKeywords
{
    public const string KiboKeywordId = "kibo";
    public const string PileMemberKeywordId = "kibo_pile_member";
}
