---
name: new-card
description: STS2 card creation and modification for the Starborn mod. Triggered by /new-card. Creates or edits card class files, localization JSON, and DynamicVars following project conventions. Use when the user wants to create a new card, modify an existing card, adjust card balance/numbers, fix card behavior, add card mechanics, or update card descriptions.
---

# STS2 Card Creation & Modification

## Determine the task type

When invoked, first ask the user whether this is a **new card** or **modifying an existing card**. If already clear from context, skip the question and proceed.

## User Shorthand

| Shorthand | Expands to |
|-----------|-----------|
| `谐调N` / `谐调N/M` | Tuning，消耗主 N / 主 N + 副 M |
| `超限X` / `超限X,N/M` | Overload，元素 X，消耗主（默认1）/ 主 N + 副 M |
| `印记N` / `印记N/M` | 获得/消耗主印记 N / 主 N + 副 M |
| `切换X` / `切X` | 切换印记属性为 X |

`N/M` = 主印记消耗 N / 副印记消耗 M。省略副值时默认 0。火/水/光 默认主属性，风/冰/木 默认副属性。

---

## Creating a New Card

1. **Gather**: card name (EN class + CN display), type, rarity, effect
2. **Research**: grep existing cards of same type/rarity for structure reference
3. **Generate**: create `.cs` in `Cards/<Rarity>/`, inheriting `StarbornCard` (or `KiboCard` for Kibo cards)
4. **Localize**: add entries to `STS2_Starborn/localization/zhs/cards.json`
5. **Register**: `[RegisterCard(typeof(StarbornCardPool))]` for character cards, `[RegisterCard(typeof(KiboCardPool))]` for Kibo cards
6. **Build**: `dotnet build sts2_starborn.sln`

---

## Creating a New Kibo (奇波)

### KiboTypeId 背景

`KiboTypeId` 是一个 `static class`，每个 Kibo 类型是一个 `const string` **本地词干**（如 `"swift_wolf"`、`"leafox"`）。背后由 `DynamicEnumValueRegistry<KiboTypeIdValue>` 提供基于 XXHash32 的确定性枚举值，用于 RitsuLib 扩展点。

- **词干是稳定标识符**：即使代码变量改名，序列化的仍是词干 → 存档不会损坏
- **限定 ID 用于跨系统引用**：`KiboTypeId.GetQualifiedId(stem)` → `"sts2_starborn_KIBOTYPE_swift_wolf"`
- **属性仍然用 stem 常量**：`[RegisterKibo(KiboTypeId.SwiftWolf)]` — `const string` 是编译期常量
- **不再有 `ToString()`**：stem 就是 string，无需转换
- **不再有 `Enum.TryParse` / `Enum.GetValues`**：用 `KiboTypeId.TryParse()` / `KiboTypeId.All`

### Checklist

| # | File(s) | What to do |
|---|---------|------------|
| 1 | `Cards/Kibo/KiboTypeId.cs` | Add `const string` stem to `KiboTypeId` + add to `All` array |
| 2 | `Cards/Kibo/KiboKeywords.cs` | Add `[RegisterOwnedCardKeyword("kibo_type_xxx", IncludeInCardHoverTip = false)]` |
| 3 | `Cards/Kibo/<Name>/Kibo<Name>RepCard.cs` | RepCard: `[RegisterKibo(KiboTypeId.Xxx)]`, `KiboCard(Power, Self)`, `KiboKeywordId` |
| 4 | `Cards/Kibo/<Name>/Kibo<Ability>Card.cs` | Normal ability: `[KiboAbilityOf(KiboTypeId.Xxx)]`, `NormalKeyword` |
| 5 | `Cards/Kibo/<Name>/Kibo<Ultimate>Card.cs` | Ultimate: `[KiboAbilityOf(KiboTypeId.Xxx, true)]`, `UltimateKeyword` |
| 6 | `Cards/<Rarity>/<PlayerCard>.cs` | Player card with `KiboSummonType`, `KiboKeywordId`, `AdditionalHoverTips` |
| 7 | `cards.json` | Titles + descriptions for all cards |

### Naming: `Kibo` + descriptive name + `Card` — never generic `Ability1`/`Ability2`.

### Stem 命名规则

Stem 使用 `snake_case`，与关键词注册后缀一致：
- `"swift_wolf"`（不是 `"SwiftWolf"`）
- `"armored_pangolin"`（不是 `"ArmoredPangolin"`）
- 单单词直接小写：`"leafox"`、`"moklido"`

### KiboTypeRegistry 中的 LocKey

`KiboTypeRegistry` 自动从 stem 生成 loc key：`$"kibo_{stem}"`。例如 stem `"swift_wolf"` → loc key `"kibo_swift_wolf"`。

### Player Card Requirements

Every player card that summons a Kibo needs four things:
- `public override string? KiboSummonType => KiboTypeId.Xxx;`
- `CanonicalKeywords`: include `KiboKeywords.KiboKeywordId.GetModCardKeyword()`
- `AdditionalHoverTips`: yield RepCard + `KiboTypeRegistry.Get(KiboTypeId.Xxx).CreatePlayableCardHoverTips()`
- `OnPlay`: `await KiboCmd.Summon(choiceContext, Owner, KiboSummonType!);`

Description prefix: `[gold]奇波[/gold]：<奇波名>。`.

> **注意**：`KiboSummonType` 现在是 `string?`（不是 `KiboTypeId?`），所以不需要 `.Value` 属性。直接 `KiboSummonType!` 即可。

---

## Modifying an Existing Card

1. Locate the `.cs` file under `Cards/`
2. Adjust `DynamicVars` declarations and/or `OnPlay` logic
3. Sync `cards.json` if the effect text changed
4. `dotnet build sts2_starborn.sln`

---

## Critical Rule: No Hardcoded Values in OnPlay

**Every number and element type in `OnPlay` MUST come from `DynamicVars`.** See [references/dynamic-vars.md](references/dynamic-vars.md) for the full type reference.

```csharp
// ✓ Correct
var dmg = DynamicVars.Damage.BaseValue;
var stacks = DynamicVars["ElementMark"].IntValue;
var elementType = ((SealElementVar)DynamicVars["ElementMark"]).ElementType;

// ✗ Wrong — upgrades won't affect these
var dmg = 6;
await StarbornCmd.Overload(ctx, slot, Owner, 1, SealElementType.Light, this);
```

Three rules:
1. **No bare integer literals** — every number is a DynamicVar
2. **Use official types** — `new CardsVar(2)`, not `new IntVar("Cards", 2)`; `new EnergyVar(3)`, not `new IntVar("Energy", 3)`. Only use `IntVar` when no specific type exists.
3. **No hardcoded `SealElementType`** — extract from `((SealElementVar)DynamicVars["ElementMark"]).ElementType`

---

## AdditionalHoverTips

When a card references a Power, generated Card, Relic, or Potion, add hover tips so players can preview them. See [references/dynamic-vars.md](references/dynamic-vars.md) for code examples.

---

## Reference Files

| File | Read when... |
|------|-------------|
| [references/dynamic-vars.md](references/dynamic-vars.md) | Choosing DynamicVar types, the hardcoding rules in full, AdditionalHoverTips code examples |
| [references/description-rules.md](references/description-rules.md) | Writing `description` strings — `diff()`, `energyIcons()`, `IfUpgraded` |
| [references/keywords-and-colors.md](references/keywords-and-colors.md) | Keyword icons, `[gold]`/`[blue]`/`[red]` conventions |
| [references/seal-element-system.md](references/seal-element-system.md) | Tuning, overload, gaining/removing marks, element type extraction pattern |
| [references/card-selection.md](references/card-selection.md) | Card picker UI — choose 1 from 3, grid, hand select |
