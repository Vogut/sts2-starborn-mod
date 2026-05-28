# Card Selection UI

When a card's effect needs the player to pick cards from a selection screen, use the appropriate `CardSelectCmd` method:

## Quick Reference

| Scenario | Method | Custom Prompt? | Max Cards |
|----------|--------|---------------|-----------|
| 3-card "choose 1" | `FromChooseACardScreen` | No | 3 |
| Multi-card grid pick | `FromSimpleGrid` | Yes (required) | Any |
| Select from hand | `FromHand` | Yes (required) | Any |
| Select from combat pile | `FromCombatPile` | Yes (required) | Any |

## Choose 1 from 3 cards (vanilla Splash pattern)

```csharp
var chosen = await CardSelectCmd.FromChooseACardScreen(
    choiceContext, cards, Owner, canSkip: false);
if (chosen != null)
    await CardPileCmd.AddGeneratedCardToCombat(chosen, PileType.Hand, Owner);
```

- No `CardSelectorPrefs` or custom localization key needed
- The screen has its own default prompt text
- `canSkip: true` allows the player to back out; use `false` to force a pick
- Returns a single `CardModel?` (null if skipped)

**Use this when**: offering exactly 3 cards for the player to choose 1 from. This is the vanilla game's standard pattern (see `Splash`).

## Multi-card grid selection (CustomizeCard pattern)

```csharp
var prefs = new CardSelectorPrefs(promptLocString, selectCount) { Cancelable = false };
var chosen = await CardSelectCmd.FromSimpleGrid(choiceContext, cards, Owner, prefs);
```

- Requires a `MenuSelectionPrompt` localisation key for the prompt `LocString`
- Supports more than 3 cards
- `CardSelectorPrefs.MinSelect` / `MaxSelect` control how many items can/must be selected

**Use this when**: the selection isn't exactly "pick 1 from 3" — e.g., more/fewer cards, or multi-select.

## Select from hand

```csharp
var prefs = new CardSelectorPrefs(CardSelectorPrefs.ExhaustSelectionPrompt, count) { Cancelable = true };
var selected = await CardSelectCmd.FromHand(choiceContext, Owner, prefs, filter, this);
```

- Use `CardSelectorPrefs.ExhaustSelectionPrompt` / `DiscardSelectionPrompt` etc. for the built-in prompt text
- `filter` can filter which cards are selectable (`null` = all cards)
- Always check the count of returned cards before using them (player may cancel)

## Adding the chosen card to hand

For cards created during combat (generated cards), use:
```csharp
await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner);
```

Not `CardPileCmd.Add(card, PileType.Hand)` — generated cards should use `AddGeneratedCardToCombat` for proper tracking.
