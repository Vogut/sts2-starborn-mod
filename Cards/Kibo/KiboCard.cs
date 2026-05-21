using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Cards;

namespace STS2_Starborn.Cards.Kibo;

public abstract class KiboCard(
    CardType type,
    TargetType targetType
) : StarbornCard(0, type, CardRarity.Token, targetType, shouldShowInCardLibrary: true)
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: Const.Paths.KiboCardPortrait(GetType()));
}
