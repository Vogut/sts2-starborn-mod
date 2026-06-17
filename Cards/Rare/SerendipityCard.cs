using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class SerendipityCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Rare, TargetType.Self
)
{
    public override string? KiboSummonType => KiboTypeId.Gururu;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            yield return KiboKeywords.KiboKeywordId.GetModCardKeyword();
            if (IsUpgraded)
                yield return CardKeyword.Retain;
        }
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId<KiboGururuRepCard>()));
            var def = KiboTypeRegistry.Get(KiboTypeId.Gururu);
            foreach (var tip in def.CreatePlayableCardHoverTips())
                yield return tip;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 选择一张手牌
        var selectPrefs = new CardSelectorPrefs(
            new LocString("cards", "STS2_STARBORN_CARD_SERENDIPITY_CARD.choicePrompt"), 1)
        {
            Cancelable = true
        };

        var selectedCards = await CardSelectCmd.FromHand(choiceContext, Owner, selectPrefs, null, this);
        var selectedCard = selectedCards.FirstOrDefault();

        if (selectedCard == null)
            return;

        // 随机设置费用 0-3
        var newCost = Owner.RunState.Rng.CombatCardSelection.NextInt(4); // 0, 1, 2, 3
        selectedCard.EnergyCost.SetThisCombat(newCost);

        // 打出这张牌
        await CardCmd.AutoPlay(choiceContext, selectedCard, cardPlay.Target);

        // 召唤奇波
        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!);
    }

    protected override void OnUpgrade()
    {
        // Retain keyword is added on upgrade
    }
}
