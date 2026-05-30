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
using STS2_Starborn.Commands;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class WolfPackHuntCard() : StarbornCard(
    2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override KiboTypeId? KiboSummonType => KiboTypeId.SwiftWolf;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<WolfPackPower>();
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId<KiboSwiftWolfRepCard>()));
            var def = KiboTypeRegistry.Get(KiboTypeId.SwiftWolf);
            foreach (var tip in def.CreatePlayableCardHoverTips())
                yield return tip;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new RepeatVar(3)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, "cardPlay.Target");
        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!.Value);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitCount(DynamicVars.Repeat.IntValue)
            .Execute(choiceContext);

        await PowerCmd.Apply<WolfPackPower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, this);

    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
