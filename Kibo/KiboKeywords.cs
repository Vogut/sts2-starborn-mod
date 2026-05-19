using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace STS2_Starborn.Kibo;

[RegisterOwnedCardKeyword("kibo")]
[RegisterOwnedCardKeyword("kibo_pile_member", IncludeInCardHoverTip = false)]
public sealed class KiboKeywords
{
    private KiboKeywords() { }

    public static readonly string KiboKeywordId =
        ModContentRegistry.GetQualifiedKeywordId(Const.ModId, "kibo");

    public static readonly string PileMemberKeywordId =
        ModContentRegistry.GetQualifiedKeywordId(Const.ModId, "kibo_pile_member");
}
