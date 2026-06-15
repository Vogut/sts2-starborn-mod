using System.Collections.Generic;
using System.Threading.Tasks;
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
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class TailwindCard() : StarbornCard(
    2, CardType.Power, CardRarity.Rare, TargetType.Self
)
{
    public override string? KiboSummonType => KiboTypeId.Phantomfly;

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            yield return KiboKeywords.KiboKeywordId.GetModCardKeyword();
            if (IsUpgraded)
                yield return CardKeyword.Innate;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        StarbornCardVars.Tuning(0, SealElementType.Wind),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<TailwindPower>();
            yield return KiboTypeRegistry.Get(KiboTypeId.Phantomfly)
                .CreateCompactCardGridHoverTips();
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Extract element type from DynamicVar — avoid hardcoding in the power
        var elementType = ((SealElementVar)DynamicVars["Tuning"]).ElementType;
        var consume = DynamicVars["Tuning"].IntValue;

        var power = await PowerCmd.Apply<TailwindPower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, this);
        if (power != null)
        {
            power.TuningElementType = elementType;
            power.TuningConsume = consume;
        }

        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!);
    }

    protected override void OnUpgrade()
    {
        // Innate keyword added in CanonicalKeywords
    }
}
