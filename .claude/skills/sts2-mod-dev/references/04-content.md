# Content: Relics, Powers, Potions, Enchantments

---

## Relics

### Basic Relic

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace MyMod.Scripts.Relics;

[RegisterRelic(typeof(MyRelicPool))]
// [RegisterCharacterStarterRelic(typeof(MyCharacter))]  // mark as starter relic
public class MyRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Common;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1)
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"{Const.Paths.Relics}/{GetType().Name}.png",
        IconOutlinePath: $"{Const.Paths.Relics}/{GetType().Name}_outline.png",
        BigIconPath: $"{Const.Paths.Relics}/{GetType().Name}_big.png"
    );

    // Draw 1 card at the start of each player turn
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        await CardPileCmd.Draw(ctx, DynamicVars.Cards.IntValue, player);
    }
}
```

### Common Relic Hooks

| Hook | When it fires |
|---|---|
| `AfterPlayerTurnStart` | Start of player's turn |
| `AfterSideTurnEnd` | End of a side's turn |
| `AfterCardPlayed` | After any card is played |
| `AfterCardDrawn` | After drawing a card |
| `AfterAttack` | After an attack resolves |
| `AfterPlayerDamaged` | When the player takes damage |
| `AfterRelicObtained` | Immediately when this relic is picked up |
| `ShouldDieLate` | Called when owner would die; return `true` to prevent death |
| `AfterPreventingDeath` | Called after ShouldDieLate returns true |

### Relic Rarities

`RelicRarity.Starter`, `RelicRarity.Common`, `RelicRarity.Uncommon`, `RelicRarity.Rare`, `RelicRarity.Boss`, `RelicRarity.Special`

### Relic Localization

File: `MyMod/localization/{lang}/relics.json`

```json
{
    "MYMOD_RELIC_MY_RELIC.title": "My Relic",
    "MYMOD_RELIC_MY_RELIC.description": "At the start of each turn, draw [blue]{Cards}[/blue] card(s).",
    "MYMOD_RELIC_MY_RELIC.flavor": "Flavor text here."
}
```

---

## Powers

Powers are persistent combat modifiers that live on a creature until removed.

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace MyMod.Scripts.Powers;

[RegisterPower]
public class MyPower : ModPowerTemplate
{
    // Buff = positive, Debuff = negative
    public override PowerType Type => PowerType.Buff;
    // Counter = stackable, Single = one instance
    public override PowerStackType StackType => PowerStackType.Counter;
    // Whether stacks can go below 0
    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"{Const.Paths.Powers}/{GetType().Name}.png",
        BigIconPath: $"{Const.Paths.Powers}/{GetType().Name}_big.png"
    );

    // Trigger: after a card is drawn
    public override async Task AfterCardDrawn(PlayerChoiceContext ctx, CardModel card, bool fromHandDraw)
    {
        await PowerCmd.Apply<StrengthPower>(Owner, Amount, Owner, null);
    }
}
```

### Apply a Power

```csharp
// In a card or relic:
await PowerCmd.Apply<MyPower>(targetCreature, stackAmount, sourceCreature, null);
```

### Power Hooks

| Hook | Typical use |
|---|---|
| `AfterPlayerTurnStart` | trigger at turn start |
| `AfterSideTurnEnd` | trigger at turn end |
| `AfterCardDrawn` | react to card draw |
| `AfterCardPlayed` | react to any card play |
| `AfterAttack` | react to attacks |
| `ModifyCardPlayCount` | change how many times a card plays |
| `ModifyHandDraw` | modify hand size |
| `ModifyMaxEnergy` | modify max energy |

### Power Localization

File: `MyMod/localization/{lang}/powers.json`

```json
{
    "MYMOD_POWER_MY_POWER.title": "My Power",
    "MYMOD_POWER_MY_POWER.description": "At the start of each turn, gain Strength.",
    "MYMOD_POWER_MY_POWER.smartDescription": "At the start of each turn, gain [blue]{Amount}[/blue] Strength."
}
```

`smartDescription` can reference `{Amount}` to show the current stack count.

---

## Potions

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace MyMod.Scripts.Potions;

[RegisterPotion(typeof(MyPotionPool))]
public class MyPotion : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Common;
    // CombatOnly = can only use in combat; AnyTime = can use anywhere
    public override PotionUsageType UsageType => PotionUsageType.CombatOnly;
    // NoTarget = no targeting required; SingleTarget = must pick enemy; etc.
    public override PotionTargetType TargetType => PotionTargetType.NoTarget;

    public override DynamicVarSet DynamicVars => new()
    {
        ModCardVars.Int("cards", 3),
    };

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: $"{Const.Paths.Images}/potions/{GetType().Name}.png",
        OutlinePath: $"{Const.Paths.Images}/potions/{GetType().Name}_outline.png"
    );

    protected override async Task OnUse(PlayerChoiceContext ctx, Player player, object? target)
    {
        await CardPileCmd.Draw(ctx, DynamicVars.Cards.IntValue, player);
    }
}
```

The `DynamicVarSet` property with `ModCardVars` is now preferred over the older `CanonicalVars`
pattern for potions — same as cards. Access via `DynamicVars.VarName.IntValue`.

```csharp
// Legacy pattern (still works):
protected override IEnumerable<DynamicVar> CanonicalVars => [
    new CardsVar(3)
];
```

### Potion Localization

File: `MyMod/localization/{lang}/potions.json`

```json
{
    "MYMOD_POTION_MY_POTION.title": "My Potion",
    "MYMOD_POTION_MY_POTION.description": "Draw [blue]{Cards}[/blue] cards."
}
```

---

## Enchantments

Enchantments modify card behavior when attached. They implement hooks on the modified card.

```csharp
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace MyMod.Scripts.Enchantments;

[RegisterEnchantment]
public class MyEnchantment : ModEnchantmentTemplate
{
    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: $"{Const.Paths.Images}/enchantments/{GetType().Name}.png"
    );

    // Called after the enchanted card's attack resolves
    public override async Task AfterAttack(PlayerChoiceContext ctx, CardPlay play,
                                           IReadOnlyList<AttackResult> results)
    {
        // Example: apply a debuff to all attacked enemies
        foreach (var result in results)
            await PowerCmd.Apply<WeakPower>(result.Target, 2, play.Card.Owner, null);
    }
}
```

Enchantments are typically applied to cards via specific relics or events in your mod logic.

### Enchantment Localization

File: `MyMod/localization/{lang}/enchantments.json`

```json
{
    "MYMOD_ENCHANTMENT_MY_ENCHANTMENT.title": "My Enchantment",
    "MYMOD_ENCHANTMENT_MY_ENCHANTMENT.description": "After attacking, apply 2 Weak."
}
```
