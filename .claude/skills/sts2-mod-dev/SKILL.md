---
name: sts2-mod-dev
description: >
  Comprehensive development guide for any Slay the Spire 2 (STS2) mod using the STS2-RitsuLib framework.
  Use this skill whenever a user wants to: build or debug any type of STS2 mod; add a playable character,
  cards, relics, powers, potions, enchantments, events, or patches; set up or troubleshoot a RitsuLib-based
  project; implement the timeline/epoch unlock system; add Godot scene assets; wire up localization;
  configure mod settings UI; implement custom card piles, combat mechanics, DataStore persistence, networking,
  audio, or top-bar buttons; or understand how any part of the game or RitsuLib API works. Also invoke when
  users mention RegisterCard, ModCharacterTemplate, TypeListCardPoolModel, ModCardTemplate, ModRelicTemplate,
  ModPowerTemplate, SavedAttachedState, ModCardVars, ContentPack, RegisterEpoch, IPatchMethod,
  RitsuLibFramework, ModTypeDiscoveryHub, RegisterOwnedCardPile, ModCardPileSpec, CardCmd, PowerCmd,
  CardPileCmd, HookPlayerChoiceContext, AttachedState, IModCardPileHandler, ModRunRngRegistry,
  ModRightClickRegistry, ModCardTransformRegistry, ModRelicVisibilityRegistry, RitsuToastService,
  RitsuGodotNodeFactories, ModModelIdentity, HarmonyIl, GetModCardKeyword, GetModCardTag,
  CardKeyword, ModKeywordRegistry, or any other RitsuLib or STS2 game API. Invoke proactively
  whenever the user is clearly building, debugging, or designing for STS2, even if they don't
  say "RitsuLib" or "mod" explicitly.
---

# STS2 Mod Development with RitsuLib

This skill covers the full spectrum of Slay the Spire 2 mod development using the **STS2-RitsuLib** framework,
from project scaffolding and new characters to custom mechanics, patches, and advanced systems. The reference
mod **STS2_WineFox** provides living examples of everything described here.

---

## ⚠ STS2-RitsuLib is under active development — Prioritize Source Code

STS2-RitsuLib is under continuous development, and the documented API may lag behind the actual implementation. **Before encountering issues or writing code**, please first check the `STS2-RitsuLib/` directory at the root of your workspace for the latest content:

- **Encountering compilation errors / API not found**: Search for the corresponding class or method name in `STS2-RitsuLib/` to confirm the current signature.
- **Preparing to write code involving the RitsuLib API**: First read the source files in the corresponding subdirectories (e.g., `CardPiles/`, `Utils/`, `Lifecycle/`) to use the current code as the reference, rather than the examples in this documentation.
- **When documentation conflicts with the actual source code**: The `STS2-RitsuLib/` source code is the ultimate authority.

> `STS2-RitsuLib/docs/` contains officially maintained documentation (Markdown), which takes precedence over this skill's reference files, but may still lag behind the latest commits. The source code (`.cs`) is always the most authoritative reference.

## 🎮 Referencing Vanilla Implementation — Using sts2/sts2 Game Source

The root directory of the workspace contains `sts2/sts2/`, which is the decompiled/original source code of the game. **When users mention "referencing vanilla content",
"how the vanilla implementation works", "keeping consistent with vanilla", or when the `RitsuLib` API cannot solve the issue, prioritize looking in this directory** rather than relying on assumptions:

- **Finding vanilla content types** (cards, relics, powers, enemies, etc.): Search for the corresponding class name directly in `sts2/sts2/`.
- **Understanding vanilla mechanics** (damage calculation, pile types, state machines, etc.): Read the vanilla implementation before designing mod logic to avoid inconsistent behavior.
- **Designing mods compatible with the original game**: Refer to the original interfaces and enum definitions (`PileType`, `CardType`, `TargetType`, etc.) to ensure your mod behaves as expected.
- **Debugging strange runtime behavior**: The original code can sometimes explain why a certain API behavior does not match the RitsuLib documentation.

> `sts2/sts2/` is read-only reference material; do not modify any files within it. When searching, prioritize exact matches for class or method names.


---

## How to use this skill

Use the section headers below to jump to the relevant stage. For each stage, if you need complete code templates or
deep-dive details, load the matching reference file from `references/`.

| Stage | When to use | Reference file |
|---|---|---|
| 1. Project Setup | brand-new mod, csproj issues, Entry.cs | `references/01-project-setup.md` |
| 2. Character & Pools | creating the character class + 3 pools | `references/02-character-and-pools.md` |
| 3. Cards | card logic, DynamicVars, base card class | `references/03-cards.md` |
| 4. Content | relics, powers, potions, enchantments | `references/04-content.md` |
| 5. Localization | JSON key naming, BBCode colors, dynamic vars | `references/05-localization.md` |
| 6. Epoch / Timeline | character unlock, content unlock milestones | `references/06-epoch.md` |
| 7. Advanced | DataStore, patches, lifecycle events, audio, events | `references/07-advanced.md` |
| 8. Settings & Diagnostics | mod settings UI, DataStore patterns, release warnings | `references/08-settings-and-diagnostics.md` |
| 9. Card Piles | custom draw/discard-style piles, UI styles, runtime access | `references/09-card-piles.md` |
| 10. New APIs (v0.3.3–0.3.5) | ModRunRng, right-click, card transforms, keyword migration, toast, and more | `references/10-new-apis.md` |

---

## Core Mental Model

RitsuLib offers two peer registration styles:

| Style | Best for |
|---|---|
| CLR attributes `[Register*]` | Content classes you own — registration stays next to the model |
| Content pack (fluent builder) | Generated content, conditional setup, or a reviewable list in one place |

Both styles require the mod assembly to be registered once in `Entry.Init()`:

```csharp
// Entry.Init() — required initialization
var assembly = Assembly.GetExecutingAssembly();
Logger = RitsuLibFramework.CreateLogger(ModId);
ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger); // only if you have .tscn-backed C# scripts

var patcher = RitsuLibFramework.CreatePatcher(ModId, "main");
patcher.RegisterPatch<MyPatch>();   // IPatchMethod — one per target
// patcher.RegisterPatches<MyPatches>();  // IModPatches — group of targets
RitsuLibFramework.ApplyRequiredPatcher(patcher, DisableMod);
```

Without `RegisterModAssembly`, none of the `[Register*]` attributes take effect.

---

## Mod Anatomy (recommended folder layout)

```
MyMod/
├── MyMod.json               ← mod manifest (id, version, dependencies)
├── MyMod.csproj             ← build config, game DLL references
├── Scripts/
│   ├── Entry.cs             ← [ModInitializer], Logger, Init()
│   ├── Const.cs             ← ModId constant, asset path helpers
│   ├── Character/
│   │   ├── MyCharacter.cs
│   │   ├── MyCardPool.cs
│   │   ├── MyRelicPool.cs
│   │   └── MyPotionPool.cs
│   ├── Cards/
│   │   ├── MyBaseCard.cs    ← optional shared base with AssetProfile shortcut
│   │   └── ...
│   ├── Powers/
│   ├── Relics/
│   ├── Potions/
│   └── Epoch/
└── MyMod/                   ← Godot resources root (matches res://MyMod/...)
    ├── images/
    │   ├── cards/
    │   ├── relics/
    │   ├── powers/
    │   └── energy_icon.png
    ├── scenes/
    │   ├── my_character.tscn
    │   └── my_energy_counter.tscn
    └── localization/
        ├── eng/          ← English (language code, not "en")
        │   ├── cards.json
        │   ├── relics.json
        │   └── powers.json
        └── zhs/          ← Simplified Chinese
            └── ...
```

> The Godot resource root **must be a subfolder named after your mod ID**, not the project root itself.
> `res://MyMod/images/...` maps to `MyMod/images/...` on disk.

---

## Creation Checklist

Walk through this list when building a new character mod:

- [ ] `MyMod.json` with `id`, `version`, `has_dll`, `has_pck`, RitsuLib dependency
- [ ] `.csproj` referencing `sts2.dll`, `0Harmony.dll`, `STS2-RitsuLib` (NuGet or local)
- [ ] `Entry.cs` with `[ModInitializer]`, Logger, and the two auto-registration lines
- [ ] Three pool classes: `TypeListCardPoolModel`, `TypeListRelicPoolModel`, `TypeListPotionPoolModel`
- [ ] Character class: `ModCharacterTemplate<CardPool, RelicPool, PotionPool>` with `[RegisterCharacter]`
- [ ] At least one base card class (Attack, Skill, Power) with `[RegisterCard(typeof(MyCardPool))]`
- [ ] Starter cards declared via `[RegisterCharacterStarterCard(typeof(MyCharacter), N)]`
- [ ] Starter relic declared via `[RegisterCharacterStarterRelic(typeof(MyCharacter))]`
- [ ] Localization JSON for every card, relic, power in every supported language
- [ ] Epoch classes for: character unlock, first-win, elite/boss milestones, ascension1
- [ ] Godot character scene (`Node2D`) with required child nodes
- [ ] Energy counter scene
- [ ] Character select background scene
- [ ] All required asset images with correct `res://` paths
- [ ] `export_presets.cfg` for PCK export

---

## Key Naming Conventions

RitsuLib derives content IDs from both the **mod ID** and the **C# class name**, both converted
to `UPPER_SNAKE_CASE`. A mod ID of `STS2_WineFox` becomes `STS2_WINE_FOX`; a class `WineFoxStrike`
becomes `WINE_FOX_STRIKE`.

| Content | ID format | Example (mod `MyMod`, class `MyStrike`) |
|---|---|---|
| Card | `{MOD_ID}_CARD_{CLASS_NAME}` | `MY_MOD_CARD_MY_STRIKE` |
| Relic | `{MOD_ID}_RELIC_{CLASS_NAME}` | `MY_MOD_RELIC_HAND_CRANK` |
| Power | `{MOD_ID}_POWER_{CLASS_NAME}` | `MY_MOD_POWER_BURNING_POWER` |
| Potion | `{MOD_ID}_POTION_{CLASS_NAME}` | `MY_MOD_POTION_HEAL_POTION` |
| Character | `{MOD_ID}_CHARACTER_{CLASS_NAME}` | `MY_MOD_CHARACTER_MY_CHARACTER` |

Use these IDs as keys in your localization JSON files (e.g., `MY_MOD_CARD_MY_STRIKE.title`).

To pin an ID so it survives class renames, set `StableEntryStem` on the attribute:
```csharp
[RegisterCard(typeof(MyCardPool), StableEntryStem = "my_strike")]
public sealed class RenamedStrike : ModCardTemplate(...) { }
```

---

## Async / Await Execution Model

STS2 uses C# `async`/`await` for all game actions. Think of each `await` as "pause here until this action
finishes, then continue." This means:

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    // These run in sequence: first deal damage, then apply power, then draw
    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
        .FromCard(this).Targeting(cardPlay.Target!).Execute(choiceContext);
    await PowerCmd.Apply<StrengthPower>(cardPlay.Target!, 2, Owner, null);
    await CardPileCmd.Draw(choiceContext, 1, Owner);
}
```

Look at the original game's cards for patterns — if a vanilla card does something similar, copy its structure.

---

## Common Commands Reference

| What you want | Command |
|---|---|
| Deal attack damage to a target | `DamageCmd.Attack(amount).FromCard(this).Targeting(target).Execute(ctx)` |
| Block | `BlockCmd.Block(amount).Source(this).Target(player).Execute(ctx)` |
| Apply a power | `PowerCmd.Apply<MyPower>(target, amount, source, null)` |
| Draw cards | `CardPileCmd.Draw(ctx, count, player)` |
| Create a card and add to hand | `CardPileCmd.CreateAndAddToHand<MyCard>(ctx, player)` |
| Exhaust a card from hand | `CardPileCmd.ExhaustCard(ctx, card)` |
| Heal | `HpCmd.Heal(creature, amount, source)` |

---

## Asset Size Guidelines

| Asset | Size |
|---|---|
| Card portrait | 250×190 (standard), 250×351 (ancient/long) |
| Energy icon (small, text) | 24×24 |
| Energy icon (big, tooltip) | 74×74 |
| Power icon (small) | 64×64 |
| Power icon (big) | 256×256 |
| Relic icon (small/outline) | 85×85 |
| Relic icon (big) | 256×256 |
| Character select icon | any reasonable size |
| Character scene root | 2560×1200 (Control node) |

---

## BBCode Color Tags in Localization

STS2 uses BBCode-style tags for colored text in descriptions:

```
[blue]{Amount}[/blue]    ← dynamic variable, shown in blue (standard for numbers)
[gold]Strength[/gold]    ← keyword or important term, shown in gold
[red]{Damage}[/red]      ← damage value
[green]{Block}[/green]   ← block value
```

---

## Pitfalls to Avoid

1. **Pool mismatch**: Always set `[RegisterCard(typeof(MyCardPool))]` on each card. If you accidentally use
   `ColorlessCardPool` the card won't appear in your character's rewards.

2. **Missing `ModTypeDiscoveryHub.RegisterModAssembly`**: Nothing auto-registers without this line. Symptoms:
   cards/relics don't appear, powers don't apply.

3. **Wrong Godot resource path**: The path must be `res://{ModId}/...`, NOT `res://{ProjectRootFolder}/...`.
   The subfolder name must match your mod ID exactly.

4. **ID collision**: Your mod ID, pool IDs, and content class names must be globally unique. Prefix everything
   with a unique mod identifier.

5. **Character requires Epoch**: By default `ModCharacterTemplate` requires a timeline story.
   If you're not ready for it yet, add `public override bool RequiresEpochAndTimeline => false;` to your
   character class.

6. **Forgetting `EnsureGodotScriptsRegistered`** when you have `.tscn` scenes: Godot scenes won't load custom
   C# scripts without this call. Pure model/patch mods that have no `.tscn` scenes can safely omit it.

7. **Version format**: Since game version `0.105.0`, all version strings must be semantic (`X.Y.Z`), not two-
   part (`X.Y`).

8. **Wrong localization language code**: Use `eng` and `zhs`, not `en` and `zh` or `en-US` and `zh-CN`.
   The folder is `localization/eng/cards.json`, not `localization/en/cards.json`.

9. **Abstract base class attributes without `Inherit = true`**: Attributes on abstract classes are ignored
   unless you explicitly add `Inherit = true`. Each concrete derived class still gets the attribute, but
   the base class itself is never registered.
