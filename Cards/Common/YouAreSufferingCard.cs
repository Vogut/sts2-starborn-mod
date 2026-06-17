using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class YouAreSufferingCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override string? KiboSummonType => KiboTypeId.Cabbird;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId<KiboCabbirdRepCard>()));
            var def = KiboTypeRegistry.Get(KiboTypeId.Cabbird);
            foreach (var tip in def.CreatePlayableCardHoverTips())
                yield return tip;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        StarbornCardVars.ElementMark(1, SealElementType.Wood),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Gain wood marks immediately
        await SealElementMarkCmd.GainElementMarks(
            choiceContext, MarkSlot.Secondary, Owner,
            DynamicVars["ElementMark"].IntValue, SealElementType.Wood);

        // Apply YouAreSufferingPower for next turn
        await PowerCmd.Apply<YouAreSufferingPower>(
            choiceContext, Owner.Creature,
            DynamicVars.Cards.IntValue,
            Owner.Creature, this);

        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ElementMark"].UpgradeValueBy(1);
    }
}
