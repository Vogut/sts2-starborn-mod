using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Runs;
using STS2_Starborn.Cards.Kibo;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace STS2_Starborn.Cards.Event;

/// <summary>
/// Summons the chosen starter Kibo, or its current evolution.
/// Added to hand by StarBoundCardRelic at combat start.
/// </summary>
[RegisterCard(typeof(TokenCardPool))]
[AncientVisual(TextBgAlpha = 0.45f, HideTypePlaque = true)]
public sealed class SummonStarterKiboCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Token, TargetType.Self, shouldShowInCardLibrary: false)
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: ResolvePortraitPath());

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            var def = ResolveStarterKiboDefinition();
            if (def == null) yield break;

            var repCardModel = ModelDb.GetById<CardModel>(ModelDb.GetId(def.RepCardType));
            yield return HoverTipFactory.FromCard(repCardModel);
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var typeId = KiboRunData.GetStarterKiboTypeId(Owner);
        if (typeId == null) return;

        await KiboCmd.Summon(choiceContext, Owner, typeId);
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        description.Add("KiboName", ResolveStarterKiboName());
    }

    private string ResolvePortraitPath()
    {
        var def = ResolveStarterKiboDefinition();
        return def == null
            ? Const.Paths.CardPortrait(GetType())
            : Const.Paths.KiboCardPortrait(def.RepCardType);
    }

    private string ResolveStarterKiboName()
    {
        var def = ResolveStarterKiboDefinition();
        if (def == null)
            return new LocString("cards", "STS2_STARBORN_CARD_SUMMON_STARTER_KIBO_CARD.fallbackKiboName")
                .GetFormattedText();

        var repCardModel = ModelDb.GetById<CardModel>(ModelDb.GetId(def.RepCardType));
        return repCardModel.Title;
    }

    private KiboTypeDefinition? ResolveStarterKiboDefinition()
    {
        if (IsCanonical)
            return null;
        if (Owner == null)
            return null;

        var typeId = KiboRunData.GetStarterKiboTypeId(Owner);
        if (typeId == null)
            return null;

        return KiboTypeRegistry.Get(typeId);
    }
}
