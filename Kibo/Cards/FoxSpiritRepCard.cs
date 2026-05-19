using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace STS2_Starborn.Kibo.Cards;

[RegisterCard(typeof(KiboCardPool))]
public sealed class FoxSpiritRepCard() : ModCardTemplate(
    0, CardType.Power, CardRarity.Token, TargetType.Self, showInCardLibrary: false)
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: Const.Paths.KiboCardPortrait(GetType()));

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }
}
