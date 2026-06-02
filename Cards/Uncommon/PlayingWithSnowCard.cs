using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class PlayingWithSnowCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.None
)
{
    public override string? KiboSummonType => KiboTypeId.SnowWolfPup;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId<KiboSnowWolfPupRepCard>()));
            var def = KiboTypeRegistry.Get(KiboTypeId.SnowWolfPup);
            foreach (var tip in def.CreatePlayableCardHoverTips())
                yield return tip;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!IsUpgraded)
            await SealElementMarkCmd.RemoveElementMarks(
                choiceContext, MarkSlot.Secondary, Owner, 1);

        await SealElementMarkCmd.SetElementType(
            choiceContext, MarkSlot.Secondary, Owner, SealElementType.Ice);

        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!);
    }

    protected override void OnUpgrade()
    {
    }
}
