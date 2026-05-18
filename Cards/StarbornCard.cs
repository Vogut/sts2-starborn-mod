using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards;

public abstract class StarbornCard(
    int energyCost,
    CardType type,
    CardRarity rarity,
    TargetType targetType,
    bool shouldShowInCardLibrary = true
) : ModCardTemplate(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
{
    protected PrimaryMarkPower? PrimaryMark => Owner.Creature.FindPower<PrimaryMarkPower>();
    protected SecondaryMarkPower? SecondaryMark => Owner.Creature.FindPower<SecondaryMarkPower>();

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: Const.Paths.CardPortrait(GetType())
        // 卡框等，有需求自己添加。需要自行判断卡牌类型（攻击、技能、能力等）设置，建议写在基类里。
        // 如果使用自定义卡池，需要改下material（TODO）
        // FramePath: "", // 卡牌背景
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
    );
}

