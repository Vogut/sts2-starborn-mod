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
| `谐调N` | Tuning，消耗主印记 N 层（= 谐调N/0） |
| `谐调N/M` | Tuning，消耗主印记 N 层 + 副印记 M 层 |
| `超限X` | Overload，元素 X，消耗主印记（默认 1/0） |
| `超限X,N/M` | Overload，元素 X，消耗主 N / 副 M |
| `印记N` | 获得/消耗 主印记 N 层（= 印记N/0） |
| `印记N/M` | 获得/消耗 主印记 N 层 + 副印记 M 层 |
| `切换X` / `切X` | 切换印记属性为 X（火/水/木） |

Implementation: for each non-zero slot, generate the corresponding command call. For example, "谐调1/1" generates two `StarbornCmd.Tuning()` calls — one for Primary, one for Secondary. Never leave the shorthand un-expanded in the output.

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

### 4. Generate Localization

Add entries to `STS2_Starborn/localization/zhs/cards.json`:
- `name`: Chinese card name
- `description`: Card effect text — read [references/description-rules.md](references/description-rules.md) for formatting
- Follow keyword and color conventions — read [references/keywords-and-colors.md](references/keywords-and-colors.md)

### 5. Register the Card

Advise the correct registration attribute:
- Character cards: `[RegisterCard(typeof(CharacterPool))]`
- Power cards: `[RegisterCard(typeof(PowerCardPool))]`
- Event cards: `[RegisterSharedEvent]`

### 6. Build & Verify

Run `dotnet build sts2_starborn.sln` and confirm no errors.

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

## Reference Files

Read these when the card being created or modified involves the relevant system:

| File | Read when... |
|------|-------------|
| [references/keywords-and-colors.md](references/keywords-and-colors.md) | Formatting any card text — keyword icons, `[gold]`/`[blue]`/`[red]` conventions |
| [references/dynamic-vars.md](references/dynamic-vars.md) | Choosing or adjusting DynamicVar types for damage, block, energy, draws, etc. |
| [references/description-rules.md](references/description-rules.md) | Writing or updating `description` strings — `diff()`, `energyIcons()`, `IfUpgraded`, standard sentence patterns |
| [references/seal-element-system.md](references/seal-element-system.md) | Card interacts with seal element marks — tuning, overload, gaining/removing stacks |
