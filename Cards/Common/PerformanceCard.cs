using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2_Starborn.Cards.Kibo;
using STS2_Starborn.Character;
using STS2_Starborn.Commands;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class PerformanceCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override KiboTypeId? KiboSummonType => KiboTypeId.Moklido;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId<KiboMoklidoRepCard>()));
            var def = KiboTypeRegistry.Get(KiboTypeId.Moklido);
            foreach (var tip in def.CreatePlayableCardHoverTips())
                yield return tip;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Dexterity", 1m),
        new BlockVar(5m, ValueProp.Move),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<AnticipatePower>(
            choiceContext, Owner.Creature,
            DynamicVars["Dexterity"].IntValue,
            Owner.Creature, this);
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!.Value);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Dexterity"].UpgradeValueBy(1m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
