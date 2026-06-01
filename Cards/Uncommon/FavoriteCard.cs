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
using STS2_Starborn.Powers;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class FavoriteCard() : StarbornCard(
    2, CardType.Power, CardRarity.Uncommon, TargetType.None
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Empower", 10),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<FavoritePower>(),
        HoverTipFactory.FromPower<KiboEmpowermentPower>(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var stacks = DynamicVars["Empower"].IntValue;
        await PowerCmd.Apply<KiboEmpowermentPower>(
            choiceContext, Owner.Creature, stacks, Owner.Creature, this);
        await PowerCmd.Apply<FavoritePower>(
            choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Empower"].UpgradeValueBy(5);
    }
}
