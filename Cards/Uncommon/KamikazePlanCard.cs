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

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class KamikazePlanCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Uncommon, TargetType.None
)
{
    public override string? KiboSummonType => KiboTypeId.VineDoll;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
        CardKeyword.Exhaust,
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Overload(2, SealElementType.Wood),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId<KiboVineDollRepCard>()));
            var def = KiboTypeRegistry.Get(KiboTypeId.VineDoll);
            foreach (var tip in def.CreatePlayableCardHoverTips())
                yield return tip;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var overloadVar = (SealElementVar)DynamicVars["Overload"];
        await StarbornCmd.Overload(choiceContext, MarkSlot.Primary, Owner,
            overloadVar.IntValue, overloadVar.ElementType, this);

        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Overload"].UpgradeValueBy(-1m);
    }
}
