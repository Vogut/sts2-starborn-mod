using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2_Starborn.Character;
using STS2_Starborn.Cards.Token;
using STS2_Starborn.Combat;
using STS2_Starborn.Element;

namespace STS2_Starborn.Cards.Common;

[RegisterCard(typeof(StarbornCardPool))]
public sealed class CustomizeCard() : StarbornCard(
    0, CardType.Skill, CardRarity.Common, TargetType.Self
)
{
    private static readonly LocString PrimaryPrompt =
        new("cards", "STS2_STARBORN_CARD_CUSTOMIZE_CARD.primaryPrompt");

    private static readonly LocString SecondaryPrompt =
        new("cards", "STS2_STARBORN_CARD_CUSTOMIZE_CARD.secondaryPrompt");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    private static CardModel GetCanonical(SealElementType et) => et switch
    {
        SealElementType.Fire => ModelDb.Card<FireMarkSelectCard>(),
        SealElementType.Water => ModelDb.Card<WaterMarkSelectCard>(),
        SealElementType.Wood => ModelDb.Card<WoodMarkSelectCard>(),
        SealElementType.Ice => ModelDb.Card<IceMarkSelectCard>(),
        SealElementType.Wind => ModelDb.Card<WindMarkSelectCard>(),
        SealElementType.Light => ModelDb.Card<LightMarkSelectCard>(),
        _ => throw new ArgumentOutOfRangeException(nameof(et), et, null)
    };

    private static List<CardModel> CreateOptions(IEnumerable<SealElementType> pool, Player owner)
    {
        var combatState = owner.Creature.CombatState;
        return pool.Select(et => combatState!.CreateCard(GetCanonical(et), owner)).ToList();
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState == null) return;

        var primaryCards = CreateOptions(ElementPoolRegistry.PrimaryPool, Owner);
        var secondaryCards = CreateOptions(ElementPoolRegistry.SecondaryPool, Owner);

        var prefs = new CardSelectorPrefs(PrimaryPrompt, 1) { Cancelable = false };
        var result = await CardSelectCmd.FromSimpleGrid(choiceContext, primaryCards, Owner, prefs);
        if (result.FirstOrDefault() is IElementChoosable p)
            await p.OnChosen(choiceContext, Owner, MarkSlot.Primary);

        prefs = new CardSelectorPrefs(SecondaryPrompt, 1) { Cancelable = false };
        result = await CardSelectCmd.FromSimpleGrid(choiceContext, secondaryCards, Owner, prefs);
        if (result.FirstOrDefault() is IElementChoosable s)
            await s.OnChosen(choiceContext, Owner, MarkSlot.Secondary);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Exhaust);
    }
}
