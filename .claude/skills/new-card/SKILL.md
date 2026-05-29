---
name: new-card
description: STS2 card creation and modification for the Starborn mod. Triggered by /new-card. Creates or edits card class files, localization JSON, and DynamicVars following project conventions. Use when the user wants to create a new card, modify an existing card, adjust card balance/numbers, fix card behavior, add card mechanics, or update card descriptions.
---

# STS2 Card Creation & Modification

## Determine the task type

When invoked, first ask the user whether this is a **new card** or **modifying an existing card**. If already clear from context, skip the question and proceed.

## User Shorthand

The user often uses compact terms. The `N/M` format means **主印记消耗N / 副印记消耗M**。When the user omits the secondary value, default to 0 (主印记 only).

| Shorthand | Expands to |
|-----------|-----------|
| `谐调N` | Tuning，消耗当前属性的主印记 N 层 |
| `谐调N/M` | Tuning，消耗当前属性的主印记 N 层 + 副印记 M 层 |
| `超限X` | Overload，元素 X，消耗当前属性的主印记（默认 1/0） |
| `超限X,N/M` | Overload，元素 X，消耗当前属性的主 N / 副 M |
| `印记N` | 获得/消耗 当前属性的主印记 N 层（= 印记N/0） |
| `印记N/M` | 获得/消耗 当前属性的主印记 N 层 + 副印记 M 层 |
| `切换X` / `切X` | 切换印记属性为 X（火/水/木） |
| `谐调（火/水/光）` | Tuning 主属性 |
| `谐调（风/冰/木）` | Tuning 副属性 |
| `超限（火/水/光）` | Overload 主属性 |
| `超限（风/冰/木）` | Overload 副属性 |

Implementation: for each non-zero slot, generate the corresponding command call. For example, "谐调1/1" generates two `StarbornCmd.Tuning()` calls — one for Primary, one for Secondary. Never leave the shorthand un-expanded in the output. 无特殊说明，火/水/光 默认为主属性，风/冰/木 默认为副属性。

---

## Creating a New Card

### 1. Gather Parameters

Ask the user for:
- **Card name** (English class name + Chinese display name)
- **Type**: Attack / Skill / Power / Event
- **Rarity**: Basic / Common / Uncommon / Rare
- **Brief effect description** (what should the card do?)

### 2. Research

- Grep existing cards of the same type/rarity to use as structural reference
- Note: base class, namespace, DynamicVars setup, and registration pattern

### 3. Generate Card Class

Create the C# file in `Cards/<Rarity>/`:
- Inherit from `StarbornCard` (or `KiboCard` for Kibo companion cards)
- Declare `DynamicVars` with proper types — read [references/dynamic-vars.md](references/dynamic-vars.md) for type selection
- Implement `OnPlay` — **every numeric value must come from `DynamicVars`** (see Critical Rule below)
- If the card references other entities (Powers, generated Cards, Relics, Potions), add `AdditionalHoverTips` — see the [AdditionalHoverTips section](#additionalhovertips--referencing-other-game-entities)

### 4. Generate Localization

Add entries to `STS2_Starborn/localization/zhs/cards.json`:
- `name`: Chinese card name
- `description`: Card effect text — read [references/description-rules.md](references/description-rules.md) for formatting
- Follow keyword and color conventions — read [references/keywords-and-colors.md](references/keywords-and-colors.md)

### 5. Register the Card

Advise the correct registration attribute:
- Character cards: `[RegisterCard(typeof(StarbornCardPool))]`
- Kibo cards (RepCard / battle skills / ultimate): `[RegisterCard(typeof(KiboCardPool))]`
- Event cards: `[RegisterSharedEvent]`

### 6. Build & Verify

Run `dotnet build sts2_starborn.sln` and confirm no errors.

---

## Creating a New Kibo (奇波) & Kibo Cards

When the user wants a new Kibo type, several files must be created together. A complete Kibo needs:

### Kibo Checklist

| Step | File(s) | What to do |
|------|---------|------------|
| 1. Enum | `Cards/Kibo/KiboTypeId.cs` | Add new value to the enum |
| 2. Keyword | `Cards/Kibo/KiboKeywords.cs` | Add `[RegisterOwnedCardKeyword("kibo_type_xxx", IncludeInCardHoverTip = false)]` |
| 3. RepCard | `Cards/Kibo/<TypeName>/Kibo<Name>Card.cs` | `[RegisterKibo(KiboTypeId.Xxx)]` on a `KiboCard(CardType.Power, TargetType.Self)` |
| 4. Battle skills | `Cards/Kibo/<TypeName>/Kibo<Name>Card.cs` | `[KiboAbilityOf(KiboTypeId.Xxx)]` with `NormalKeyword` |
| 5. Ultimate | `Cards/Kibo/<TypeName>/Kibo<Name>Card.cs` | `[KiboAbilityOf(KiboTypeId.Xxx, true)]` with `UltimateKeyword` |
| 6. Player card | `Cards/<Rarity>/` | The card that summons this Kibo (see below) |
| 7. Localization | `cards.json` | Title for RepCard + descriptions for all Kibo cards + player card |

### Naming Convention

**Kibo cards MUST use `Kibo` + descriptive name + `Card`** — never generic `Ability1`/`Ability2`/`Ultimate`:

```
KiboClawCard      ✓  爪击
KiboHowlCard      ✓  狼嚎
KiboPincerCard    ✓  围攻
```
```
SwiftWolfAbility1Card  ✗  (too generic)
```

The RepCard follows the same pattern: `KiboSwiftWolfCard`, `KiboMoklidoCard`.

### Kibo Card Templates

**RepCard** — Minimal; just the `[RegisterKibo]` marker:
```csharp
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace STS2_Starborn.Cards.Kibo;

[RegisterCard(typeof(KiboCardPool))]
[RegisterKibo(KiboTypeId.Xxx)]
public sealed class KiboXxxCard() : KiboCard(CardType.Power, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
    }
}
```

**Battle skill** (normal ability) — `[KiboAbilityOf]` + `NormalKeyword`:
```csharp
[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.Xxx)]
public sealed class KiboYyyCard() : KiboCard(CardType.Attack, TargetType.AnyEnemy)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [ KiboKeywords.NormalKeyword ];
    // ... DynamicVars + OnPlay
}
```

**Ultimate skill** — `[KiboAbilityOf(Xxx, true)]` + `UltimateKeyword`:
```csharp
[RegisterCard(typeof(KiboCardPool))]
[KiboAbilityOf(KiboTypeId.Xxx, true)]
public sealed class KiboZzzCard() : KiboCard(...);
```

### Player Card That Summons a Kibo

When a player card is associated with a Kibo (e.g. 奇波：迅狼), it **must** include ALL of:

```csharp
[RegisterCard(typeof(StarbornCardPool))]
public sealed class SomeCard() : StarbornCard(...)
{
    // 1. Kibo association — triggers auto-registration when card enters deck
    public override KiboTypeId? KiboSummonType => KiboTypeId.Xxx;

    // 2. Show "奇波" keyword icon on the card face
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        KiboKeywords.KiboKeywordId.GetModCardKeyword(),
    ];

    // 3. Hover tips: show the Kibo's RepCard + all playable cards
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromCard(
                ModelDb.GetById<CardModel>(ModelDb.GetId<KiboXxxCard>()));
            var def = KiboTypeRegistry.Get(KiboTypeId.Xxx);
            foreach (var tip in def.CreatePlayableCardHoverTips())
                yield return tip;
        }
    }

    protected override async Task OnPlay(...)
    {
        // 4. Summon the Kibo in combat — use KiboSummonType!.Value, never hardcode
        await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!.Value);
        // ... rest of card effect
    }
}
```

### Localization for Kibo Cards

- **Player card description**: always prefix with `[gold]奇波[/gold]：<奇波名>。` — matches `FoxSpiritCallCard` convention
- **RepCard**: needs both `.title` and `.description` (description can be `""`)
- **Battle skills / Ultimate**: always lead with `[gold]奇波战技[/gold]。` or `[gold]奇波绝技[/gold]。`

### Key Namespaces for Kibo Cards

| Namespace | For |
|-----------|-----|
| `STS2_Starborn.Cards.Kibo` | `KiboTypeId`, `KiboKeywords`, `KiboTypeRegistry`, `KiboCard`, `KiboCardPool` |
| `STS2_Starborn.Commands` | `KiboCmd`, `SealElementMarkCmd` |
| `STS2_Starborn.Combat` | `MarkSlot` |
| `STS2_Starborn.Element` | `SealElementType` |
| `STS2_Starborn.Character` | `StarbornCardPool` |
| `STS2RitsuLib.Keywords` | `.GetModCardKeyword()` extension |
| `MegaCrit.Sts2.Core.Models` | `ModelDb` (for `AdditionalHoverTips`) |

---

## Modifying an Existing Card

### 1. Locate the Card

- Find the card class file by name under `Cards/`
- Read the current implementation to understand its structure

### 2. Understand the Change

Clarify what the user wants to modify:
- **Numeric tuning**: adjust DynamicVar base values, add/remove upgrade deltas
- **Mechanics change**: alter OnPlay logic, add/remove effect steps
- **Description fix**: update localization text to match behavior
- **Keyword/type change**: add/remove CanonicalKeywords, change rarity/pool

### 3. Edit the Code

- Modify `DynamicVars` declarations if values change
- Edit `OnPlay` logic — keep all numeric values sourced from `DynamicVars`
- If adding new mechanics that involve seal elements, read [references/seal-element-system.md](references/seal-element-system.md)

### 4. Update Localization

- Sync `STS2_Starborn/localization/zhs/cards.json` if the effect text changed
- If adding new DynamicVars, ensure the description references them correctly — read [references/description-rules.md](references/description-rules.md)

### 5. Build & Verify

Run `dotnet build sts2_starborn.sln` and confirm no errors.

---

## Critical Rule: No Hardcoded Values in OnPlay

All numeric values in `OnPlay` **must** come from `DynamicVars`:
```csharp
// Correct
var damage = DynamicVars.Damage.BaseValue;
var block = DynamicVars.Block.BaseValue;
var stacks = DynamicVars["ElementMark"].IntValue;

// Wrong — upgrades won't affect these
var damage = 6;
var block = 5;
```

Reason: upgrades work by modifying `DynamicVar.BaseValue`. Hardcoded numbers ignore upgrades entirely.

## AdditionalHoverTips — Referencing Other Game Entities

When a card's effect references another game entity that the player may not know, add `AdditionalHoverTips` so the entity can be previewed directly from the card's tooltip. This applies whenever the card **applies a Power, summons/generates other Cards, creates a Relic/Potion**, or otherwise references a named entity beyond common keywords.

### Trigger conditions

Ask: "would the player reading this card's description want to see what X does?" If yes, add a hover tip for X.

| Card does this... | Add hover tip for... | Factory method |
|---|---|---|
| Applies a buff/debuff (Power) | That power | `HoverTipFactory.FromPower<ThePower>()` |
| Generates or offers other cards | Each card type | `HoverTipFactory.FromCard(cardModel)` |
| Creates or references a Relic | That relic | `HoverTipFactory.FromRelic(relicModel)` |
| Creates or references a Potion | That potion | `HoverTipFactory.FromPotion(potionModel)` |

### Code examples

**Power reference** (most common):
```csharp
using MegaCrit.Sts2.Core.HoverTips;

protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
[
    HoverTipFactory.FromPower<RubyIsTier0Power>(),
];
```

**Multiple card references** (e.g. generating Token cards):
```csharp
private static readonly Type[] TokenCardTypes = [typeof(AxeCard), typeof(PlankCard), /*...*/];

protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    TokenCardTypes.Select(t => HoverTipFactory.FromCard(
        ModelDb.GetById<CardModel>(ModelDb.GetId(t))));
```

### When NOT to add

- The referenced entity is a **common game keyword** already known to all players (e.g. Weak, Vulnerable, Strength)
- The card description **already fully explains the effect** inline without naming another entity
- The reference is purely cosmetic or narrative with no gameplay impact

Place `AdditionalHoverTips` alongside `CanonicalKeywords` and `CanonicalVars` in the card class body.

---

## Reference Files

Read these when the card being created or modified involves the relevant system:

| File | Read when... |
|------|-------------|
| [references/keywords-and-colors.md](references/keywords-and-colors.md) | Formatting any card text — keyword icons, `[gold]`/`[blue]`/`[red]` conventions |
| [references/dynamic-vars.md](references/dynamic-vars.md) | Choosing or adjusting DynamicVar types for damage, block, energy, draws, etc. |
| [references/description-rules.md](references/description-rules.md) | Writing or updating `description` strings — `diff()`, `energyIcons()`, `IfUpgraded`, standard sentence patterns |
| [references/seal-element-system.md](references/seal-element-system.md) | Card interacts with seal element marks — tuning, overload, gaining/removing stacks |
| [references/card-selection.md](references/card-selection.md) | Card shows a card picker UI — choose 1 from 3, grid select, hand/exhaust select |
