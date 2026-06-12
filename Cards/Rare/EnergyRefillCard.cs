using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Combat;
using STS2_Starborn.Commands;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Rare;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class EnergyRefillCard() : StarbornCard(
    1, CardType.Skill, CardRarity.Rare, TargetType.Self
)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ElementMark(1, SealElementType.Any, "ElementMark", MarkSlot.Primary),
        ElementMark(1, SealElementType.Any, "ElementMarkSecondary", MarkSlot.Secondary),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var discardPile = PileType.Discard.GetPile(Owner);
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, 0, 3) { Cancelable = true };
        var selected = (await CardSelectCmd.FromCombatPile(choiceContext, discardPile, Owner, prefs)).ToList();
        if (selected.Count == 0) return;

        var perCard = DynamicVars["ElementMark"].IntValue;
        var totalStacks = selected.Count * perCard;
        foreach (var card in selected)
            await CardCmd.Exhaust(choiceContext, card);
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Primary, Owner, totalStacks);
        await SealElementMarkCmd.GainElementMarks(choiceContext, MarkSlot.Secondary, Owner, totalStacks);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
