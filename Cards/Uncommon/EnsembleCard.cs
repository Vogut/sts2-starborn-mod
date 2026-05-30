using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class EnsembleCard() : StarbornCard(
    4, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override KiboTypeId? KiboSummonType => KiboTypeId.MelodiousVine;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId<KiboMelodiousVineRepCard>()));
            var def = KiboTypeRegistry.Get(KiboTypeId.MelodiousVine);
            foreach (var tip in def.CreatePlayableCardHoverTips())
                yield return tip;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(9, ValueProp.Move),
    ];

    public override async Task AfterCardEnteredCombat(CardModel card)
    {
        await base.AfterCardEnteredCombat(card);
        if (card == this)
        {
            ElementMarkState.MarksChanged += UpdateCost;
            UpdateCost();
        }
    }

    public override async Task BeforeCardRemoved(CardModel card)
    {
        if (card == this)
            ElementMarkState.MarksChanged -= UpdateCost;
        await base.BeforeCardRemoved(card);
    }

    private void UpdateCost()
    {
        var switchedCount = ElementMarkManager.GetSwitchedTypeCount(Owner);
        EnergyCost.SetThisCombat(System.Math.Max(0, 4 - switchedCount));
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!.Value);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
