# Localization — STS2 RitsuLib Mod

STS2 localization is driven by JSON files stored in `{ModId}/localization/{lang}/`.
Each content type has its own file. Keys are derived from auto-generated content IDs.

---

## File Structure

```
MyMod/
└── localization/
    ├── eng/                ← English  (code: "eng", NOT "en")
    │   ├── cards.json
    │   ├── relics.json
    │   ├── powers.json
    │   ├── potions.json
    │   ├── enchantments.json
    │   ├── characters.json
    │   ├── card_keywords.json
    │   ├── static_hover_tips.json
    │   ├── ancients.json
    │   ├── epochs.json
    │   └── events.json
    └── zhs/                ← Simplified Chinese (code: "zhs")
        └── ...  (same file names, different text)
```

Language codes: **`eng`** (English), **`zhs`** (Simplified Chinese), **`jpn`** (Japanese).
These are the game's own locale codes — not ISO 639-1 (`en`, `zh`, `ja`).

---

## ID → Key Derivation

RitsuLib generates IDs from both your **mod ID** and **class name**, both converted to `UPPER_SNAKE_CASE`.
A mod named `MyMod` → prefix `MY_MOD`; a class `MyStrike` → suffix `MY_STRIKE`.

```
Mod ID:     "MyMod"       → MY_MOD
Class:       MyStrike      → MY_STRIKE
Full ID:     MY_MOD_CARD_MY_STRIKE
```

| Class (mod `MyMod`) | Generated ID |
|---|---|
| `MyStrike` | `MY_MOD_CARD_MY_STRIKE` |
| `MyStarterRelic` | `MY_MOD_RELIC_MY_STARTER_RELIC` |
| `MyBurnPower` | `MY_MOD_POWER_MY_BURN_POWER` |
| `MyHealPotion` | `MY_MOD_POTION_MY_HEAL_POTION` |
| `MyFireEnchant` | `MY_MOD_ENCHANTMENT_MY_FIRE_ENCHANT` |
| `MyCharacter` | `MY_MOD_CHARACTER_MY_CHARACTER` |

To pin an ID across class renames, set `StableEntryStem` on the attribute — see [02-character-and-pools.md].

---

## Table of Localization Files by Model Type

| Model | File | Common keys |
|---|---|---|
| Card | `cards` | `.title`, `.description`, `.selectionScreenPrompt` |
| Relic | `relics` | `.title`, `.description`, `.flavor`, `.selectionScreenPrompt` |
| Potion | `potions` | `.title`, `.description`, `.selectionScreenPrompt` |
| Power | `powers` | `.title`, `.description` |
| Character | `characters` | `.title`, pronouns, messages (see below) |
| Enchantment | `enchantments` | `.title`, `.description` |
| Custom keyword | `card_keywords` | `.title`, `.description` |
| Hover tips / shared terms | `static_hover_tips` | `.title`, `.description`, `.empty` (for card piles) |
| Story event | `events` | `.pages.<PAGE>.description`, `.pages.<PAGE>.options.<OPTION>` |
| Ancient event | `ancients` | event page keys + `talk` dialogue keys |
| Epoch | `epochs` | `.title`, `.description`, `.unlockInfo`, `.unlockText` |

---

## cards.json

```json
{
    "MY_MOD_CARD_MY_STRIKE.title": "Strike",
    "MY_MOD_CARD_MY_STRIKE.description": "Deal [red]{Damage}[/red] damage.",
    "MY_MOD_CARD_MY_DEFEND.title": "Defend",
    "MY_MOD_CARD_MY_DEFEND.description": "Gain [green]{Block}[/green] Block."
}
```

Upgrade descriptions are handled automatically via `{Damage:diff()}` — the `:diff()` formatter
highlights values that differ from the base card in the upgrade preview.

---

## relics.json

```json
{
    "MY_MOD_RELIC_MY_STARTER_RELIC.title": "My Starter Relic",
    "MY_MOD_RELIC_MY_STARTER_RELIC.description": "At the start of each turn, draw [blue]{Cards}[/blue] card(s).",
    "MY_MOD_RELIC_MY_STARTER_RELIC.flavor": "It hums with energy."
}
```

---

## powers.json

```json
{
    "MY_MOD_POWER_MY_POWER.title": "Empowered",
    "MY_MOD_POWER_MY_POWER.description": "Gain [blue]{Amount}[/blue] Strength whenever you draw a card."
}
```

---

## characters.json (full key list)

This file requires more keys than other content types. All keys use the character's entry ID as prefix:

```json
{
    "MY_MOD_CHARACTER_MY_CHARACTER.title": "My Character",
    "MY_MOD_CHARACTER_MY_CHARACTER.description": "A short description shown in character select.",
    "MY_MOD_CHARACTER_MY_CHARACTER.flavor": "Same or alternate flavor text.",
    "MY_MOD_CHARACTER_MY_CHARACTER.selectMessage": "Let's do this.",
    "MY_MOD_CHARACTER_MY_CHARACTER.victoryMessage": "That's how it's done.",
    "MY_MOD_CHARACTER_MY_CHARACTER.defeatMessage": "I'll be back stronger.",

    "MY_MOD_CHARACTER_MY_CHARACTER.pronounSubject": "she",
    "MY_MOD_CHARACTER_MY_CHARACTER.pronounObject": "her",
    "MY_MOD_CHARACTER_MY_CHARACTER.possessiveAdjective": "her",
    "MY_MOD_CHARACTER_MY_CHARACTER.pronounPossessive": "hers",

    "MY_MOD_CHARACTER_MY_CHARACTER.goldMonologue": "A little extra gold is always welcome.",
    "MY_MOD_CHARACTER_MY_CHARACTER.eventDeathPrevention": "Not yet — I can still keep going.",

    "MY_MOD_CHARACTER_MY_CHARACTER.cardsModifierTitle": "My Character Cards",
    "MY_MOD_CHARACTER_MY_CHARACTER.cardsModifierDescription": "My Character cards now appear in rewards and shops.",

    "MY_MOD_CHARACTER_MY_CHARACTER.unlockText": "Win a run as [pink]{Prerequisite}[/pink] to unlock.",

    "MY_MOD_CHARACTER_MY_CHARACTER.banter.alive.endTurnPing": "Your turn.",
    "MY_MOD_CHARACTER_MY_CHARACTER.banter.dead.endTurnPing": "I'm down."
}
```

---

## epochs.json

```json
{
    "MY_CHARACTER_EPOCH.title": "My Character",
    "MY_CHARACTER_EPOCH.description": "A new adventurer enters the Spire.",
    "MY_CHARACTER_EPOCH.unlockInfo": "Complete a run",
    "MY_CHARACTER_EPOCH.unlockText": "Unlock My Character",

    "MY_FIRST_WIN_EPOCH.title": "First Victory",
    "MY_FIRST_WIN_EPOCH.description": "Three new cards unlocked.",
    "MY_FIRST_WIN_EPOCH.unlockInfo": "Win a run as My Character",
    "MY_FIRST_WIN_EPOCH.unlockText": "First Steps"
}
```

---

## card_keywords.json (custom card-level keywords)

```json
{
    "MY_MOD_KEYWORD_HEAT.title": "Heat",
    "MY_MOD_KEYWORD_HEAT.description": "Some cards change behavior based on current Heat."
}
```

Register the keyword through the content pack or a registry:

```csharp
RitsuLibFramework.CreateContentPack("MyMod")
    .KeywordOwned("HEAT")
    .Apply();
// or directly:
RitsuLibFramework.GetKeywordRegistry("MyMod").RegisterCardKeywordOwnedByLocNamespace("HEAT");
```

Reference in card descriptions: `"Use [gold]Heat[/gold] once."`

---

## static_hover_tips.json (shared hover tooltips)

Use `static_hover_tips` for custom terms that need hover tooltips but are not card-level keywords.
This is also the file for card pile titles and top-bar buttons.

```json
{
    "MY_MOD_HEAT.title": "Heat",
    "MY_MOD_HEAT.description": "Some cards care about the current Heat value.",

    "MY_MOD_MATERIAL_PILE.title": "Materials",
    "MY_MOD_MATERIAL_PILE.description": "Resources collected during combat.",
    "MY_MOD_MATERIAL_PILE.empty": "No materials yet."
}
```

Attach a tooltip to a `ModCardVars` variable:

```csharp
ModCardVars.Int("heat", Heat)
    .WithSharedTooltip("MY_MOD_HEAT", "res://MyMod/images/ui/heat.png");
```

---

## events.json (story event pages)

For event entry `MY_MOD_EVENT_QUIET_DOOR`:

```json
{
    "MY_MOD_EVENT_QUIET_DOOR.pages.INITIAL.description": "A quiet door waits in the wall.",
    "MY_MOD_EVENT_QUIET_DOOR.pages.INITIAL.options.OPEN": "[Open] Step through.",
    "MY_MOD_EVENT_QUIET_DOOR.pages.INITIAL.options.LEAVE": "[Leave] Walk away.",
    "MY_MOD_EVENT_QUIET_DOOR.pages.DONE.description": "The room is quiet again."
}
```

Keep page and option names stable after release — they are part of the player's event-choice history.

---

## BBCode Color Reference

| Tag | Color | Typical use |
|---|---|---|
| `[blue]...[/blue]` | Blue | Numeric values shown as dynamic vars |
| `[gold]...[/gold]` | Gold | Keyword names, important terms |
| `[red]...[/red]` | Red | Damage values |
| `[green]...[/green]` | Green | Block values |
| `[gray]...[/gray]` | Gray | Flavor or secondary info |
| `[pink]...[/pink]` | Pink | Character names in unlock text |


---

## Character Names & UI Text

Character name, select screen flavor text, and other UI strings are registered through the
character's epoch localization. Refer to the epoch system (see `references/06-epoch.md`)
for how to add story text and character description strings.
