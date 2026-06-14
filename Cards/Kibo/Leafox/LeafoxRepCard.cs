using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Element;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[RegisterKibo(KiboTypeId.Leafox, EvolvesTo = KiboTypeId.Floratail, IsStarter = true, Element = SealElementType.Wood)]
public sealed class LeafoxRepCard() : KiboCard(-1, CardType.Power, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: Const.Paths.CardPortrait(GetType()),
        FramePath: "res://STS2_Starborn/cards/WaterFrame.png", // 自定义卡框贴图
        FrameMaterial: MaterialUtils.CreateUnmodulatedHsvShaderMaterial() // 保持原图颜色，不灰度化
        // PortraitBorderPath: "", // 边框（状态牌感染使用的）
        // BannerTexturePath: "" // 横幅（不同类型）
    );
}
