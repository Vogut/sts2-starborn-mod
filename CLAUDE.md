# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

The mod builds via the Godot.NET.Sdk. Open `sts2_starborn.sln` in an IDE, or build from CLI:

```powershell
dotnet build sts2_starborn.sln
```

Post-build targets in [sts2_starborn.csproj](sts2_starborn.csproj) automatically:
1. Copy `sts2_starborn.dll` and `sts2_starborn.json` to `$(Sts2Dir)/mods/sts2_starborn/`
2. Run Godot headless to export the `.pck` asset pack to the same mods folder

Key paths configured in the `.csproj`:
- `Sts2Dir`: `D:\Steam\steamapps\common\Slay the Spire 2`
- `GodotExe`: `D:\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64\GodotSharp.exe`

NuGet dependency: `STS2.RitsuLib` (the modding framework).

## Architecture

This is a **Slay the Spire 2 mod** built on the **STS2-RitsuLib** framework. It adds a custom character, **Starborn (星临者)**, with two core mechanics: **Seal Elements (元素印记)** and **Kibo (奇波)** companion creatures.

### Project Map

| Directory | Purpose |
|---|---|
| `Cards/` | All card classes — basic, common/uncommon/rare, event cards, Kibo subsystem cards |
| `Character/` | Starborn character definition, card/relic/potion pool types |
| `Combat/` | Combat-scoped singletons: `ElementMarkManager`, `KiboCombatManager` |
| `Commands/` | Static command helpers that execute gameplay logic (seal marks, tuning/overload, Kibo summon) |
| `Element/` | Seal Element types (Fire/Water/Wood/None) with Tuning/Overload behavior |
| `Events/` | Custom map events (Kibo starter choice, AzurLane easter egg) |
| `Hooks/` | Hook listener interfaces + static dispatchers (event bus pattern for mod interop) |
| `Localization/` | SmartFormat formatter for rendering element icons in card text |
| `Map/` | Custom act map that inserts a Kibo starter node |
| `Patches/` | Harmony patches via `IPatchMethod` (RitsuLib's patching abstraction) |
| `Powers/` | Power (buff/debuff) classes |
| `Relics/` | Relic classes |
| `Runs/` | Per-player persistent run data: Kibo collection, element mark state |
| `UI/` | Godot Control widgets: element mark display, Kibo portrait/pedestal widget |
| `STS2_Starborn/` | Godot assets root — scenes, textures, localization JSON (zhs) |
| `STS2-RitsuLib/` | Git submodule — RitsuLib framework source (excluded from compilation, consumed as NuGet) |
| `sts2/` | Git submodule — game source stubs for reference (not compiled) |

### Registration Pattern

All game content is registered via **RitsuLib attributes** scanned at startup. Mod initialization in [Entry.cs](Entry.cs):

1. Register Godot scripts via `RitsuLibFramework.EnsureGodotScriptsRegistered()`
2. Register this assembly with `ModTypeDiscoveryHub.RegisterModAssembly()` for auto-discovery
3. Initialize subsystems (KiboTypeRegistry, run data, card piles)
4. Register Harmony patches via `RitsuLibFramework.CreatePatcher()` + `ApplyRequiredPatcher()`

Key attributes used throughout:
- `[RegisterCard(typeof(Pool))]` / `[RegisterRelic(typeof(Pool))]` / `[RegisterPower]`
- `[RegisterCharacter]` on the `Starborn` class
- `[RegisterCharacterStarterCard(typeof(Char), N)]` / `[RegisterCharacterStarterRelic(typeof(Char))]`
- `[RegisterSharedEvent]` for map events
- `[RegisterKibo(...)]` for Kibo type definitions (a custom attribute defined in this mod)
- `[RegisterSingleton]` on combat managers (`HookedSingletonModel`)
- `[RegisterOwnedCardKeyword(...)]` for custom keywords
- `[RegisterSmartFormatter]` for custom string formatters

### Base Class Hierarchy

All player cards inherit from `StarbornCard` (which extends `ModCardTemplate`). This base adds:
- Seal element mark helpers (`CanTuning`, `CanOverload`)
- Kibo summon lifecycle hooks (`TriggerAutoSummon`, `OnSummonKibo`, `OnKiboCardPlayed`)

Kibo companion cards inherit from `KiboCard` (extends `StarbornCard`) — 0-cost Token rarity cards that auto-play. Their boss-room evolution is handled in the base class.

Relics inherit from `StarbornRelic` (`ModRelicTemplate`); powers from `StarbornPower` (`ModPowerTemplate`).

### The Two Core Mechanics

**Seal Elements (元素印记)**: Players accumulate element "marks" in combat via `ElementMarkManager`. There are four element types (Fire/Water/Wood/None) in primary and secondary slots. At end of each player turn, marks trigger **Tuning** (threshold) and **Overload** (enhanced) effects, dispatched through the hook system in [Commands/StarbornCmd.cs](Commands/StarbornCmd.cs).

**Kibo (奇波)**: Equippable companion creatures defined by `KiboTypeId` in [Cards/Kibo/KiboTypeId.cs](Cards/Kibo/KiboTypeId.cs). Each Kibo type has abilities (cards) and an evolution chain. The active Kibo auto-plays its abilities at end of turn via `KiboCombatManager`. Kibo cards go to a custom card pile managed by `KiboPileManager`. The collection persists across runs via `KiboRunData`.

### Hook System

Custom listener interfaces in [Hooks/](Hooks/) follow an event-bus + static dispatcher pattern. Classes implement interfaces like `ISealElementMarkListener` or `IKiboSwitchListener`, then the corresponding `*Hooks` static class dispatches to all registered listeners. This allows cards, relics, and powers to react to mechanic events without coupling.

### Commands vs Managers

Commands are static stateless helpers that execute discrete gameplay actions (e.g., `SealElementMarkCmd.SetElementType()`, `KiboCmd.AutoPlay()`). They handle hook dispatch and validation. Managers are combat-scoped singletons (`[RegisterSingleton]` on `HookedSingletonModel`) that own state and orchestrate timing (e.g., when to trigger end-of-turn effects).

### Archaic Tooth / Touch of Orobas

The mod registers transcendence/refinement mappings in [Entry.cs](Entry.cs) for meta-progression card/relic upgrades via:
```csharp
RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<TestCard, TestAncientCard>();
RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<TestRelic, TestRelic>();
```

### Localization

All user-facing strings use SmartFormat. The mod is localized to Simplified Chinese (zhs) with JSON files in `STS2_Starborn/localization/zhs/`. The custom `SealElementIconsFormatter` renders inline element icons in card descriptions via `{element:Fire}` syntax.

### Card Dynamic Variables

Custom dynamic vars in [Cards/StarbornCardVars.cs](Cards/StarbornCardVars.cs) and [Cards/SealElementVar.cs](Cards/SealElementVar.cs) define values that appear with special formatting in card tooltips and scale with game state.

### Tracing Framework / Game Source References

When you need to understand how a framework API (RitsuLib) or game API (STS2) is used — e.g., how `ModCardTemplate` lifecycle methods are overridden, or where `DamageCmd` is called — use a task agent to trace all references to that symbol across both the mod codebase and the submodules (`STS2-RitsuLib/`, `sts2/`). The agent should report back every implementation and its call sites so you have the full picture before making changes.
