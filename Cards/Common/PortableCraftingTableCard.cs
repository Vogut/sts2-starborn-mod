using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.RunRngs;
using STS2RitsuLib.Scaffolding.Content;
using STS2_Starborn.Character;
using STS2_Starborn.Cards.Token;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class PortableCraftingTableCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Common, TargetType.Self
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
        TokenCardTypes.Select(t => HoverTipFactory.FromCard(
            ModelDb.GetById<CardModel>(ModelDb.GetId(t))));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = Owner!.Creature.CombatState;
        if (combatState == null) return;

        var exhaustPrefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 2) { Cancelable = true };
        var toExhaust = (await CardSelectCmd.FromHand(choiceContext, Owner, exhaustPrefs, null, this)).ToList();
        if (toExhaust.Count < 2) return;

        foreach (var card in toExhaust)
            await CardCmd.Exhaust(choiceContext, card);

        var rng = ModRunRngRegistry.Get(Owner, Const.ModId, "portable_crafting_table");
        var pickedTypes = TokenCardTypes.OrderBy(_ => rng.NextDouble()).Take(3).ToList();

        var options = pickedTypes.Select(t =>
        {
            var canonical = ModelDb.GetById<CardModel>(ModelDb.GetId(t));
            var card = combatState.CreateCard(canonical, Owner);
            if (IsUpgraded)
                CardCmd.Upgrade(card);
            return card;
        }).ToList();

        var chosen = await CardSelectCmd.FromChooseACardScreen(choiceContext, options, Owner);
        if (chosen != null)
            await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
    }
}
