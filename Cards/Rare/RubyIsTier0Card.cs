using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public class RubyIsTier0Card() : StarbornCard(
    2, CardType.Power, CardRarity.Rare, TargetType.None
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Int("Limit", 4),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<RubyIsTier0Power>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RubyIsTier0Power>(
            choiceContext, Owner.Creature, DynamicVars["Limit"].IntValue, Owner.Creature, this);
        await SealElementMarkCmd.SetElementType(
            choiceContext, MarkSlot.Primary, Owner, SealElementType.Fire);
        await SealElementMarkCmd.SetElementType(
            choiceContext, MarkSlot.Secondary, Owner, SealElementType.Fire);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Limit"].UpgradeValueBy(2);
    }
}
