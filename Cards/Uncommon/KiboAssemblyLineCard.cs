using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Cards.Token;
using STS2_Starborn.Powers;
using STS2_Starborn.UI;

namespace STS2_Starborn.Cards.Uncommon;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class KiboAssemblyLineCard() : StarbornCard(
    2, CardType.Power, CardRarity.Uncommon, TargetType.None
)
{
    private static readonly Type[] TokenCardTypes =
    [
        typeof(FragileAxeCard),
        typeof(ElementToolboxCard),
        typeof(LowStarboundCard),
        typeof(SturdyPlankCard),
        typeof(FragilePickaxeCard),
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new IntVar("Count", 1),
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<KiboAssemblyLinePower>(),
        new CompactCardGridHoverTip(
            TokenCardTypes.Select(t =>
                ModelDb.GetById<CardModel>(ModelDb.GetId(t)))),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<KiboAssemblyLinePower>(
            choiceContext, Owner.Creature, DynamicVars["Count"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Count"].UpgradeValueBy(1);
    }
}
