# Cards — STS2 RitsuLib Mod

---

## Base Card Class (recommended)

Create one shared abstract base to avoid repeating `AssetProfile` on every card:

```csharp
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace MyMod.Scripts.Cards;

public abstract class MyBaseCard : ModCardTemplate
{
    // Object-initializer style: only fill in what you have; missing fields use defaults
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = $"{Const.Paths.Cards}/{GetType().Name}.png",
        EnergyIconPath = Const.Paths.EnergyIcon,
    };

    protected MyBaseCard(int cost, CardType type, CardRarity rarity,
                         TargetType target = TargetType.None,
                         bool showInLibrary = true)
        : base(cost, type, rarity, target, showInLibrary) { }
}
```

All concrete cards inherit `MyBaseCard` instead of `ModCardTemplate`.

To pin a card's public entry ID (so renaming the class doesn't break saves), set `StableEntryStem`:

```csharp
[RegisterCard(typeof(MyCardPool), StableEntryStem = "my_strike")]
public sealed class RenamedStrike : MyBaseCard { ... }
```

To apply an attribute to all cards in a base class, set `Inherit = true`:

```csharp
[RegisterCard(typeof(MyCardPool), Inherit = true)]
public abstract class MyBaseCard : ModCardTemplate { ... }
// All non-abstract subclasses automatically get [RegisterCard(typeof(MyCardPool))]
```

---

## Card Anatomy

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;

namespace MyMod.Scripts.Cards;

[RegisterCard(typeof(MyCardPool))]                         // registers to your pool
// [RegisterCharacterStarterCard(typeof(MyCharacter), 5)] // include in starter deck
public class MyStrikeCard : MyBaseCard
{
    private const int EnergyCost = 1;
    private const CardType Type = CardType.Attack;
    private const CardRarity Rarity = CardRarity.Basic;
    private const TargetType Target = TargetType.AnyEnemy;

    // DynamicVars — use ModCardVars API (preferred over CanonicalVars)
    public override DynamicVarSet DynamicVars => new()
    {
        ModCardVars.Int("damage", Damage),
    };

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(play.Target!)
            .Execute(ctx);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);  // 6 → 9 on upgrade
    }
}
```

---

## DynamicVar Variables — ModCardVars API

Use `ModCardVars` inside the `DynamicVarSet` property to define values shown in descriptions.

```csharp
public override DynamicVarSet DynamicVars => new()
{
    ModCardVars.Int("damage", Damage),           // simple int, uses existing Damage field
    ModCardVars.Int("block", Block),
    ModCardVars.Int("cards", 2),                 // fixed integer
    ModCardVars.Computed("bonus", 0,             // computed from card state
        card => card?.Upgraded == true ? 5 : 0),
    ModCardVars.Int("heat", Heat)               // with hover tooltip
        .WithSharedTooltip("MY_MOD_HEAT", "res://MyMod/images/ui/heat.png"),
};
```

Reference in description JSON: `"Deal {damage} damage. Gain {block} Block."`

Safe reading from external code:

```csharp
var amount = card.DynamicVars.GetIntOrDefault("damage");
var hasHeat = card.DynamicVars.HasPositiveValue("heat");
```

`ValueProp` (used in old `DamageVar` / `BlockVar` constructors) is a bitflag enum:
- `ValueProp.Move` — standard damage affected by Strength, etc.
- `ValueProp.Unpowered` — ignores all modifiers
- `ValueProp.Unblockable` — bypasses block

---

## Card Types

| `CardType` | Description |
|---|---|
| `Attack` | Attack cards (red banner) |
| `Skill` | Skill cards (green banner) |
| `Power` | Power cards (purple banner) — played once, grants ongoing power |
| `Status` | Status cards (not normally playable) |
| `Curse` | Curse cards |

---

## Card Rarities

`CardRarity.Basic`, `CardRarity.Common`, `CardRarity.Uncommon`, `CardRarity.Rare`, `CardRarity.Ancient`

Ancient cards use a taller portrait (250×351 px).

---

## Common Card Patterns

### Multi-hit attack

```csharp
protected override IEnumerable<DynamicVar> CanonicalVars => [
    new DamageVar(4, ValueProp.Move),
    new MagicVar(3)   // hit count
];

protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    for (int i = 0; i < DynamicVars.Magic.IntValue; i++)
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(play.Target!).Execute(ctx);
}
```

### AoE attack (all enemies)

```csharp
private const TargetType Target = TargetType.AllEnemies;

protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    foreach (var enemy in ctx.GetOpponentCreatures())
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this).Targeting(enemy).Execute(ctx);
}
```

### Block + apply power to self

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    await BlockCmd.Block(DynamicVars.Block.BaseValue)
        .Source(this).Target(Owner).Execute(ctx);
    await PowerCmd.Apply<StrengthPower>(Owner, DynamicVars.Magic.IntValue, Owner, null);
}
```

### Draw cards

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    await CardPileCmd.Draw(ctx, DynamicVars.Cards.IntValue, Owner);
}
```

### Exhaust self on play

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    await DamageCmd.Attack(...).Execute(ctx);
    await CardPileCmd.ExhaustCard(ctx, this);
}
```

### Create a card and add to hand

```csharp
protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
{
    await CardPileCmd.CreateAndAddToHand<MyTokenCard>(ctx, Owner);
}
```

---

## Ancient / Upgraded Variants

Ancient cards are the "super upgrade" variant unlocked via the Archaic Tooth relic.
Create a separate class inheriting from the base card, override what changes:

```csharp
[RegisterCard(typeof(MyCardPool))]
public class MyAncientStrikeCard : MyStrikeCard
{
    public MyAncientStrikeCard() : base(...)
    {
        // Ancient cards are typically pre-upgraded
        IsUpgraded = true;
    }

    // Ancient cards use taller portrait (250×351)
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"{Const.Paths.Cards}/Ancient/{GetType().Name}.png"
    );
}
```

Register the mapping in `Entry.Init()`:
```csharp
RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<MyStrikeCard, MyAncientStrikeCard>();
```

---

## Token Cards (generated during combat)

Token cards (not in the player's deck, created mid-combat) should be registered to a
`TokenCardPool`-like pool or be marked as non-library cards:

```csharp
[RegisterCard(typeof(MyCardPool))]
public class MyTokenCard : MyBaseCard
{
    public MyTokenCard() : base(0, CardType.Attack, CardRarity.Common,
                                TargetType.AnyEnemy, shouldShowInCardLibrary: false) { }
    ...
}
```

---

## Card Localization Key Format

The auto-generated ID for class `MyStrike` in mod `MyMod` is `MY_MOD_CARD_MY_STRIKE`.
Both the mod ID and class name are converted to `UPPER_SNAKE_CASE` individually.

```json
{
    "MY_MOD_CARD_MY_STRIKE.title": "Strike",
    "MY_MOD_CARD_MY_STRIKE.description": "Deal {damage} damage."
}
```

File: `MyMod/localization/eng/cards.json` (English) or `localization/zhs/cards.json` (Simplified Chinese).

Language codes: `eng` (English), `zhs` (Simplified Chinese), `jpn` (Japanese) — **not** `en`/`zh`/`ja`.
