using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Cards.Token;
using STS2_Starborn.UI;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class AlphaTestGiftCard() : StarbornCard(
    2, CardType.Skill, CardRarity.Rare, TargetType.Self
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

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        new CompactCardGridHoverTip(
            TokenCardTypes.Select(t =>
                ModelDb.GetById<CardModel>(ModelDb.GetId(t)))),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        foreach (var type in TokenCardTypes)
        {
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(type));
            var card = combatState.CreateCard(canonical, Owner);
            if (IsUpgraded)
                CardCmd.Upgrade(card);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        // Upgrade changes behavior in OnPlay (upgraded cards), no numeric changes
    }
}
