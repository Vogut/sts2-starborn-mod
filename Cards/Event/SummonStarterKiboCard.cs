using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;
using STS2_Starborn.Runs;
using STS2_Starborn.Cards.Kibo;

namespace STS2_Starborn.Cards.Event;

/// <summary>
/// Summons the chosen starter Kibo, or its current evolution.
/// Added to hand by StarBoundCardRelic at combat start.
/// </summary>
[RegisterCard(typeof(StarbornCardPool))]
public sealed class SummonStarterKiboCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Token, TargetType.Self, shouldShowInCardLibrary: false)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            if (Owner == null) yield break;

            var typeId = KiboRunData.GetStarterKiboTypeId(Owner);
            if (typeId == null) yield break;

            var def = Kibo.KiboTypeRegistry.Get(typeId);
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
}
