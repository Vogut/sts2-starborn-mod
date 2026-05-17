# Character & Pools — STS2 RitsuLib Mod

Every character mod needs exactly **three pool classes** plus the character class itself.
Pools tell the game which cards/relics/potions belong to your character.

---

## Pool Classes

All three pools follow the same pattern — inherit from `TypeList*PoolModel` and configure
your energy icon + color theming.

### Card Pool

```csharp
using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace MyMod.Scripts.Character;

public class MyCardPool : TypeListCardPoolModel
{
    // Unique pool ID — must not clash with other mods
    public override string Title => "mymod";
    public override string EnergyColorName => "mymod";

    // Energy icon shown inline in card text descriptions (24×24)
    public override string? TextEnergyIconPath => Const.Paths.EnergyIcon;
    // Energy icon shown in tooltip and card top-left corner (74×74)
    public override string? BigEnergyIconPath => Const.Paths.EnergyIconBig;

    // Theme color for the deck viewer background
    public override Color DeckEntryCardColor => new(0.5f, 0.5f, 1f);
    // Energy dial text outline color
    public override Color EnergyOutlineColor => new(0.5f, 0.5f, 1f);

    // Tints the default card frame to your theme color — remove if you use a custom frame
    private static readonly Material? _poolFrameMaterial =
        MaterialUtils.CreateRgbShaderMaterial(0.5f, 0.5f, 1f);
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    public override bool IsColorless => false;
}
```

### Relic Pool

```csharp
using STS2RitsuLib.Scaffolding.Content;

namespace MyMod.Scripts.Character;

public class MyRelicPool : TypeListRelicPoolModel
{
    public override string? TextEnergyIconPath => Const.Paths.EnergyIcon;
    public override string? BigEnergyIconPath => Const.Paths.EnergyIconBig;
    public override string EnergyColorName => "mymod";
}
```

### Potion Pool

```csharp
using STS2RitsuLib.Scaffolding.Content;

namespace MyMod.Scripts.Character;

public class MyPotionPool : TypeListPotionPoolModel
{
    public override string? TextEnergyIconPath => Const.Paths.EnergyIcon;
    public override string? BigEnergyIconPath => Const.Paths.EnergyIconBig;
    public override string EnergyColorName => "mymod";
}
```

---

## Character Class

```csharp
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using STS2RitsuLib.Data.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace MyMod.Scripts.Character;

[RegisterCharacter]
public sealed class MyCharacter : ModCharacterTemplate<MyCardPool, MyRelicPool, MyPotionPool>
{
    public override Color NameColor => new(0.5f, 0.5f, 1f);
    public override Color EnergyLabelOutlineColor => new(0.5f, 0.5f, 1f);
    public override Color MapDrawingColor => new(0.5f, 0.5f, 1f);

    public override CharacterGender Gender => CharacterGender.Feminine;
    public override int StartingHp => 75;
    public override int StartingGold => 99;

    // AssetProfile: object-initializer style — only fill in what you have
    public override CharacterAssetProfile AssetProfile => new()
    {
        Scenes = new()
        {
            VisualsPath = Const.Paths.Scenes + "/my_character.tscn",
            EnergyCounterPath = Const.Paths.Scenes + "/my_energy_counter.tscn",
        },
        Ui = new()
        {
            CharacterSelectIconPath = Const.Paths.Images + "/char_select.png",
            CharacterSelectBgPath = Const.Paths.Scenes + "/my_character_select_bg.tscn",
            MapMarkerPath = Const.Paths.Images + "/map_marker.png",
        },
        Audio = new()
        {
            AttackSfx = "event:/MyMod/character_attack",
        },
        // PlaceholderCharacterId = "ironclad",  // ← dev scaffold only: borrows missing fields
                                                  //   from a vanilla character; remove before release
    };

    // Comment this out once you have a timeline set up (see references/06-epoch.md)
    public override bool RequiresEpochAndTimeline => false;

    // Required: lets RitsuLib auto-create the character visuals from the tscn path
    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);
}
```

### Visibility & Progression Properties

Override these when the character should not behave exactly like a built-in character:

| Property | Effect |
|---|---|
| `RequiresEpochAndTimeline` | Set `false` to skip vanilla epoch/timeline requirements (dev mode) |
| `HideFromVanillaCharacterSelect` | Hide from the vanilla character-select screen |
| `AllowInVanillaRandomCharacterSelect` | Control random character selection |
| `HideInCardLibraryCompendium` | Hide the character's card-pool filter in the compendium |
| `CardLibraryCompendiumPlacementRules` | Place the card pool near another pool or in custom order |

### Starter Content — Attribute Style (preferred)

Declare starter cards and relics with attributes on each class. The `count` argument adds that many copies:

```csharp
[RegisterCard(typeof(MyCardPool))]
[RegisterCharacterStarterCard(typeof(MyCharacter), count: 4, Order = 10)]
public sealed class MyStrike : ModCardTemplate(1, CardType.Attack, CardRarity.Common, TargetType.SingleEnemy)
{ ... }

[RegisterRelic(typeof(MyRelicPool))]
[RegisterCharacterStarterRelic(typeof(MyCharacter), Order = 0)]
public sealed class MyStarterRelic : ModRelicTemplate { ... }
```

`Order` controls ordering within the starter list. Lower = earlier; ties keep registration order.

### Starter Content — Content Pack Style (alternative)

Use the content pack builder when you want the full starter list visible in one place:

```csharp
RitsuLibFramework.CreateContentPack("MyMod")
    .Character<MyCharacter>(c => c
        .AddStartingRelic<MyStarterRelic>(1, order: 0)
        .AddStartingCard<MyStrike>(4, order: 10)
        .AddStartingCard<MyDefend>(4, order: 20))
    .Card<MyCardPool, MyStrike>()
    .Card<MyCardPool, MyDefend>()
    .Relic<MyRelicPool, MyStarterRelic>()
    .Apply();
```

Do **not** mix content-pack starter registration with the old `StartingDeckTypes` / `StartingRelicTypes` overrides.

### Optional: Orobas / Ancient Tooth Mappings

Register with attributes on the card/relic class:

```csharp
[RegisterCard(typeof(MyCardPool))]
[RegisterArchaicToothTranscendence(typeof(MyAncientStartCard))]
public sealed class MyStartCard : ModCardTemplate(...) { ... }

[RegisterRelic(typeof(MyRelicPool))]
[RegisterTouchOfOrobasRefinement(typeof(MyUpgradedRelic))]
public sealed class MyStarterRelic : ModRelicTemplate { ... }
```

---

## Godot Character Scene Structure

Create `my_character.tscn` as a `Node2D` scene with this exact hierarchy
(the `%` means "Unique Name" — right-click node → "Access as Unique Name"):

```
MyCharacter (Node2D)
├── Visuals (Node2D)           %  ← root visual container
├── Bounds (Control)           %  ← collision/hitbox area
├── IntentPos (Marker2D)       %  ← position for intent indicators
├── CenterPos (Marker2D)       %  ← center of character for effects
└── TalkPos (Marker2D)         %  ← position for speech bubbles
```

Attach your character's sprite/AnimationPlayer under `Visuals`. The sizes and positions
of the markers should be tuned to match your character sprite's dimensions.

---

## Character Select Background Scene

Create `my_character_select_bg.tscn` as a `Control` node (root size: **2560×1200**).
This is the full-screen background shown when the character is highlighted in character select.
Add background artwork, particles, or animated elements as children.

---

## Energy Counter Scene

The energy counter (the dial showing current/max energy) can be:
- Copied from an existing character's scene and adapted, or
- Created from scratch following the same node structure as Ironclad's counter.

Reference the Ironclad energy counter scene in the game files as a starting point.
