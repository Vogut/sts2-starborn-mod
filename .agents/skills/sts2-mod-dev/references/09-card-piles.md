# Card Piles — STS2 RitsuLib Mod

Custom card piles extend the vanilla draw / discard / exhaust model. Each pile is registered once,
receives a deterministically minted `PileType` enum value, and is lazily instantiated per combat or run.

---

## Qualified ID Format

`{NORMALIZED_MOD_ID}_CARDPILE_{NORMALIZED_STEM}`

| Example | Result |
|---|---|
| mod `com.example.mymmod`, stem `overflow` | `MYMMOD_CARDPILE_OVERFLOW` |
| mod `MyMod`, stem `overflow_pile` | `MYMOD_CARDPILE_OVERFLOW_PILE` |

This ID is the stem for localization keys in `static_hover_tips.json`:
- `MYMOD_CARDPILE_OVERFLOW.title`
- `MYMOD_CARDPILE_OVERFLOW.description`
- `MYMOD_CARDPILE_OVERFLOW.empty` (shown in thought bubble when the pile is empty and clicked)

> **v0.3.5:** `GetPileType()` now uses deterministic minting — any string ID produces a valid `PileType` without
> pre-registration. Registration is still required for UI configuration (icon, style, anchor).

---

## Registration — Attribute Style

Place `[RegisterOwnedCardPile]` on any class in your mod assembly. The class is the registration carrier
and can optionally implement `IModCardPileHandler` to customize the button-click behavior.

```csharp
[RegisterOwnedCardPile("overflow",
    Scope  = ModCardPileScope.CombatOnly,
    Style  = ModCardPileUiStyle.BottomLeft,
    AnchorKind = ModCardPileAnchorKind.BottomLeftSecondary,
    IconPath = "res://art/overflow_pile.png")]
public sealed class OverflowPileRegistration : IModCardPileHandler
{
    // Called when the player clicks the pile button (non-empty pile only)
    public void OnOpen(ModCardPileOpenContext ctx)
    {
        ctx.ShowDefaultPileScreen(); // open the standard pile view
        // or: ctx.OpenCapstoneScreen(new MyCustomScreen());
    }
}
```

`IModCardPileHandler` is **optional**. If omitted, clicking the button opens the default
`NCardPileScreen` automatically.

> **Limitation**: `VisibleWhen`, `FlightTargetPositionResolver`, and `FlightStartPositionResolver`
> **cannot** be set via attributes — use the direct registry if you need delegate-based visibility or
> custom card-flight positions.

---

## Registration — Content Pack / Direct Registry

```csharp
// In Entry.Init()
RitsuLibFramework.CreateContentPack("MyMod")
    .CardPileOwned("overflow", new ModCardPileSpec
    {
        Scope    = ModCardPileScope.CombatOnly,
        Style    = ModCardPileUiStyle.BottomLeft,
        Anchor   = new ModCardPileAnchor(ModCardPileAnchorKind.BottomLeftSecondary),
        IconPath = "res://art/overflow_pile.png",
        OnOpen   = ctx => ctx.ShowDefaultPileScreen(),
        VisibleWhen = ctx => ctx.Pile?.Count > 0, // hide button when empty
    })
    .Apply();

// Or via direct registry:
RitsuLibFramework.GetCardPileRegistry("MyMod")
    .RegisterOwned("overflow", new ModCardPileSpec { ... });
```

---

## `ModCardPileSpec` Properties

| Property | Type | Default | Notes |
|---|---|---|---|
| `Scope` | `ModCardPileScope` | `CombatOnly` | `CombatOnly` = per-combat; `RunPersistent` = per-run |
| `Style` | `ModCardPileUiStyle` | `Headless` | Controls which UI chrome is added (see table below) |
| `Anchor` | `ModCardPileAnchor` | `Default` | Explicit slot; falls back to auto-stack in registration order |
| `IconPath` | `string?` | null | `res://` Godot path; falls back to placeholder |
| `Hotkeys` | `string[]?` | null | Hotkey ids that open the pile view |
| `CardShouldBeVisible` | `bool` | false | Only meaningful for `ExtraHand` — renders cards as NCard nodes |
| `VisibleWhen` | `Func<ModCardPileVisibilityContext, bool>?` | null (always shown) | Button visibility predicate, checked per `_Process` tick |
| `OnOpen` | `Action<ModCardPileOpenContext>?` | null (default screen) | Custom button-click handler |
| `HoverTipPlacement` | `ModCardPileHoverTipPlacement` | `Auto` | Hover tip anchor relative to the button |
| `HoverTipScreenOffset` | `Vector2` | `Zero` | Fine-tune hover tip position |
| `FlightTargetPositionResolver` | `Func<ModCardPileFlightTargetContext, Vector2?>?` | null | Dynamic fly-in target for cards entering the pile |
| `FlightStartPositionResolver` | `Func<ModCardPileFlightStartContext, Vector2?>?` | null | Dynamic fly-out source for cards leaving the pile |

---

## UI Styles (`ModCardPileUiStyle`)

| Value | Where it appears |
|---|---|
| `Headless` | No UI button; cards still fly to the pile's coordinate. Used for invisible holding piles. |
| `BottomLeft` | Button row on the bottom-left (next to Draw pile). Auto-stacks rightward. |
| `BottomRight` | Button row on the bottom-right (next to Exhaust pile). Auto-stacks leftward. |
| `TopBarDeck` | Top bar, left of the vanilla deck button. |
| `ExtraHand` | Hand-like card container, rendered above or below the player's hand. |

---

## Anchor Kinds (`ModCardPileAnchorKind`)

| Value | Meaning |
|---|---|
| `StyleDefault` | Auto-stack within the style's default row/position |
| `BottomLeftPrimary` | Near the draw pile; stacks rightward |
| `BottomLeftSecondary` | Near the discard pile; stacks rightward on overflow |
| `BottomRightPrimary` | Near the exhaust pile; stacks leftward |
| `BottomRightSecondary` | Reserved second bottom-right slot |
| `TopBarAfterDeck` | Immediately after the vanilla deck button |
| `TopBarBeforeModifiers` | Before the right-most modifier cluster |
| `ExtraHandAbove` | Centered above the player's hand |
| `ExtraHandBelow` | Centered below the player's hand |
| `Custom` | User-specified position (`CustomPosition` + `CustomAuthoringPivot`) |

---

## Scope (`ModCardPileScope`)

| Value | Lifetime | Storage |
|---|---|---|
| `CombatOnly` | Disposed when combat ends | `PlayerCombatState` — included in `AllPiles` sweep |
| `RunPersistent` | Survives across combats within a run | `Player` — in-memory only (serialization is WIP) |

---

## Runtime Access

RitsuLib patches `CardPile.Get()`, so vanilla-style access works transparently:

```csharp
// From any card/power/relic handler — get the live pile for the current player
var pile = player.GetPile("MYMOD_CARDPILE_OVERFLOW".GetModCardPileType());
// pile is a ModCardPile (subclass of CardPile), or null if scope unavailable

// Move a card into the pile (vanilla CardPile API)
if (pile != null)
{
    player.PlayerCombatState.MoveCard(card, pile);
}

// Draw from the pile (vanilla CardPile API)
player.PlayerCombatState.MoveCard(pile!.TopCard, player.PlayerCombatState.Hand);
```

**Helper extensions** (`ModCardPileExtensions`):

```csharp
// ID → PileType
PileType type = "MYMOD_CARDPILE_OVERFLOW".GetModCardPileType();

// PileType → ID (for diagnostics / serialization)
if (type.TryGetModCardPileId(out var id)) { ... }
```

---

## Localization — `static_hover_tips.json`

Card pile hover tips are always resolved from the game's built-in `static_hover_tips` table.
Mods **cannot** create a new loc table — extend the existing one via the mod localization pipeline.

```
localization/
  eng/
    static_hover_tips.json   ← add pile keys here
```

```json
{
  "MYMOD_CARDPILE_OVERFLOW.title": "Overflow",
  "MYMOD_CARDPILE_OVERFLOW.description": "Cards that cannot fit in your hand go here.",
  "MYMOD_CARDPILE_OVERFLOW.empty": "The overflow pile is empty."
}
```

---

## Common Pitfalls

- **`VisibleWhen` returning wrong value during scene wiring** — both `Player` and `Pile` in
  `ModCardPileVisibilityContext` can be null while the combat UI initializes. Guard against null and
  return `false` (or `true` for always-visible) when state is not yet available.
- **`RunPersistent` piles refill empty on run load** — persistence/serialization for run-scoped piles
  is not yet implemented; do not rely on them surviving a save/reload.
- **Wrong loc table** — do not create a new loc table; all pile strings must go in `static_hover_tips.json`.
- **Attribute cannot supply delegates** — `VisibleWhen`, `FlightTargetPositionResolver`, and
  `FlightStartPositionResolver` require the direct registry; attributes only expose value-type fields.
- **`ExtraHand` cards visible flag** — `CardShouldBeVisible = true` is required if you want cards to
  render as interactive NCard nodes inside an `ExtraHand`-style pile.
