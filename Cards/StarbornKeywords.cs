using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace STS2_Starborn.Cards;

[RegisterOwnedCardKeyword("bounce")]
public class StarbornKeywords
{
    public static readonly string BounceKeywordId =
        ModContentRegistry.GetQualifiedKeywordId(Const.ModId, "bounce");

    public static readonly CardKeyword BounceKeyword = BounceKeywordId.GetModCardKeyword();
}
